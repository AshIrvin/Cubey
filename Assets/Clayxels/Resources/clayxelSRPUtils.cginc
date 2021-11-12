
#ifdef SHADERPASS // detect the shadow pass in HDRP and URP
	#if SHADERPASS == SHADERPASS_SHADOWS
		#define SHADERPASS_SHADOWCASTER
	#endif
#endif

#ifdef UNITY_PASS_SHADOWCASTER // detect shadow pass in built-in
	#define SHADERPASS_SHADOWCASTER
#endif

#if defined (SHADER_API_D3D11) || defined(SHADER_API_METAL) || (SHADER_API_GLES) || (SHADER_API_GLES3) || (SHADER_API_PS4) || (SHADER_API_XBOXONE)
	#define CLAYXELS_VALID
#endif

#ifdef CLAYXELS_VALID
	uniform StructuredBuffer<int2> chunkPoints;
	uniform StructuredBuffer<float3> chunksCenter;
	uniform StructuredBuffer<int> pointCloudDataToSolidId;
	uniform StructuredBuffer<int> pointToChunkId;
#endif 

float4x4 objectMatrix;
float chunkSize = 0.0;
float splatRadius = 0.01;
int solidHighlightId;
float lod = 0.0;

uint _RoughPattern = 5;
float _RoughColor = 0.0;
float _RoughBump = 0.0;
float _RoughOrient = 0.0;

static const float vOffsetUpTable[3] = {-1.0, -1.0, 1.7};
static const float vOffsetSideTable[3] = {1.0, -1.0, 0.0};
static const float4 vTexTable[3] = {
	 float4(-0.5, 0.0, 0.0, 0.0),
	 float4(1.5, 0.0, 0.0, 0.0),
	 float4(0.5, 1.35, 0.0, 0.0)
};

int bytes4ToInt(uint a, uint b, uint c, uint d){
	int retVal = (a << 24) | (b << 16) | (c << 8) | d;
	return retVal;
}

int4 unpackInt4(uint inVal){
	uint r = inVal >> 24;
	uint g = (0x00FF0000 & inVal) >> 16;
	uint b = (0x0000FF00 & inVal) >> 8;
	uint a = (0x000000FF & inVal);

	return int4(r, g, b, a);
}

float3 unpackFloat3(float f){
	return frac(f / float3(16777216, 65536, 256));
}

float4 unpackR6G6B6A14(uint value){
	float a = ((float(value & 0x3FFF) / 16383) * 2.0) - 1.0;
	value >>= 14;
	float b = float(value & 0x3f) / 63;
	value >>= 6;
    float g = float(value & 0x3f) / 63;
    float r = float(value >> 6) / 63;
   	
    return float4(r, g, b, a);
}


float3 expandGridPoint(int3 cellCoord, float cellSize, float localChunkSize){
	float cellCornerOffset = cellSize * 0.5;
	float halfBounds = localChunkSize * 0.5;
	float3 gridPoint = float3(
		(cellSize * cellCoord.x) - halfBounds, 
		(cellSize * cellCoord.y) - halfBounds, 
		(cellSize * cellCoord.z) - halfBounds) + cellCornerOffset;

	return gridPoint;
}

float2 unpackFloat2(float input){
	int precision = 32;
	float2 output = float2(0.0, 0.0);

	output.y = input % precision;
	output.x = floor(input / precision);

	return output / (precision - 1);
}

float3 unpackNormal(float fSingle){
	float2 f = unpackFloat2(fSingle);

	f = f * 2.0 - 1.0;

	float3 n = float3( f.x, f.y, 1.0 - abs( f.x ) - abs( f.y ) );
	float t = saturate( -n.z );
	n.xy += n.xy >= 0.0 ? -t : t;

	return normalize( n );
}

float3 unpackNormal2Byte(uint value1, uint value2){
	float2 f = float2((float)value1 / 256.0, (float)value2 / 256.0);

	f = f * 2.0 - 1.0;

	float3 n = float3( f.x, f.y, 1.0 - abs( f.x ) - abs( f.y ) );
	float t = saturate( -n.z );
	n.xy += n.xy >= 0.0 ? -t : t;

	return normalize( n );
}

float3 unpackRgb(uint inVal){
	int r = (inVal & 0x000000FF) >>  0;
	int g = (inVal & 0x0000FF00) >>  8;
	int b = (inVal & 0x00FF0000) >> 16;

	return float3(r/255.0, g/255.0, b/255.0);
}

void unpack66668(int value, out int4 unpackedData1, out int unpackedData2){
	unpackedData2 = value & 0x000000FF;
	value >>= 8;
	unpackedData1.w = value & 0x3f;
	value >>= 6;
	unpackedData1.z = value & 0x3f;
	value >>= 6;
	unpackedData1.y = value & 0x3f;
	value >>= 6;
	unpackedData1.x = value & 0x3f;
}

int clayxelGetChunkId(uint pointId){
#ifdef CLAYXELS_VALID
	uint arrayId = pointId / 5;
	uint localOffset = 6 * round(float(float(pointId) / 5.0 - arrayId) * 5);

	uint packedValue = pointToChunkId[arrayId];

	uint chunkId = (((1 << 6) - 1) & (packedValue >> localOffset)); 

	return chunkId;
#else
	return 0;
#endif
}


void clayxelGetPointCloud(uint vId, out float3 gridPoint, out float3 pointColor, out float3 pointCenter, out float3 pointNormal){
#ifdef CLAYXELS_VALID
	int pointId = vId / 3;
	int2 clayxelPointData = chunkPoints[pointId];
	
	int4 data1 = unpackInt4(clayxelPointData.x);
	int4 data2;
	int data3;
	unpack66668(clayxelPointData.y, data2, data3);

	float3 normal = unpackNormal2Byte(data1.w, data3);

	float cellSize = chunkSize / 256.0;
	float halfCell = cellSize * 0.5;

	float normalOffset = (((data2.x / 64.0) * 4.0) - 1.0) * halfCell;
	
	float3 cellOffset = float3(cellSize*0.5, cellSize*0.5, cellSize*0.5) + (normal * normalOffset);

	gridPoint = expandGridPoint(data1.xyz, cellSize, chunkSize);

	int chunkId = clayxelGetChunkId(pointId);
	
	float3 chunkCenter = chunksCenter[chunkId];

	float3 pointPos = gridPoint + cellOffset + chunkCenter;
	pointCenter = mul(objectMatrix, float4(pointPos, 1.0)).xyz;
	
	pointNormal = mul((float3x3)objectMatrix, normal);
	
	pointColor = float3((float)data2.y / 64.0, (float)data2.z / 64.0, (float)data2.w / 64.0);
#else
	gridPoint = float3(0, 0, 0);
	pointColor = float3(0, 0, 0);
	pointCenter = float3(0, 0, 0);
	pointNormal = float3(0, 0, 0);
#endif
}

float4x4 inverse(float4x4 m) {
    float n11 = m[0][0], n12 = m[1][0], n13 = m[2][0], n14 = m[3][0];
    float n21 = m[0][1], n22 = m[1][1], n23 = m[2][1], n24 = m[3][1];
    float n31 = m[0][2], n32 = m[1][2], n33 = m[2][2], n34 = m[3][2];
    float n41 = m[0][3], n42 = m[1][3], n43 = m[2][3], n44 = m[3][3];

    float t11 = n23 * n34 * n42 - n24 * n33 * n42 + n24 * n32 * n43 - n22 * n34 * n43 - n23 * n32 * n44 + n22 * n33 * n44;
    float t12 = n14 * n33 * n42 - n13 * n34 * n42 - n14 * n32 * n43 + n12 * n34 * n43 + n13 * n32 * n44 - n12 * n33 * n44;
    float t13 = n13 * n24 * n42 - n14 * n23 * n42 + n14 * n22 * n43 - n12 * n24 * n43 - n13 * n22 * n44 + n12 * n23 * n44;
    float t14 = n14 * n23 * n32 - n13 * n24 * n32 - n14 * n22 * n33 + n12 * n24 * n33 + n13 * n22 * n34 - n12 * n23 * n34;

    float det = n11 * t11 + n21 * t12 + n31 * t13 + n41 * t14;
    float idet = 1.0f / det;

    float4x4 ret;

    ret[0][0] = t11 * idet;
    ret[0][1] = (n24 * n33 * n41 - n23 * n34 * n41 - n24 * n31 * n43 + n21 * n34 * n43 + n23 * n31 * n44 - n21 * n33 * n44) * idet;
    ret[0][2] = (n22 * n34 * n41 - n24 * n32 * n41 + n24 * n31 * n42 - n21 * n34 * n42 - n22 * n31 * n44 + n21 * n32 * n44) * idet;
    ret[0][3] = (n23 * n32 * n41 - n22 * n33 * n41 - n23 * n31 * n42 + n21 * n33 * n42 + n22 * n31 * n43 - n21 * n32 * n43) * idet;

    ret[1][0] = t12 * idet;
    ret[1][1] = (n13 * n34 * n41 - n14 * n33 * n41 + n14 * n31 * n43 - n11 * n34 * n43 - n13 * n31 * n44 + n11 * n33 * n44) * idet;
    ret[1][2] = (n14 * n32 * n41 - n12 * n34 * n41 - n14 * n31 * n42 + n11 * n34 * n42 + n12 * n31 * n44 - n11 * n32 * n44) * idet;
    ret[1][3] = (n12 * n33 * n41 - n13 * n32 * n41 + n13 * n31 * n42 - n11 * n33 * n42 - n12 * n31 * n43 + n11 * n32 * n43) * idet;

    ret[2][0] = t13 * idet;
    ret[2][1] = (n14 * n23 * n41 - n13 * n24 * n41 - n14 * n21 * n43 + n11 * n24 * n43 + n13 * n21 * n44 - n11 * n23 * n44) * idet;
    ret[2][2] = (n12 * n24 * n41 - n14 * n22 * n41 + n14 * n21 * n42 - n11 * n24 * n42 - n12 * n21 * n44 + n11 * n22 * n44) * idet;
    ret[2][3] = (n13 * n22 * n41 - n12 * n23 * n41 - n13 * n21 * n42 + n11 * n23 * n42 + n12 * n21 * n43 - n11 * n22 * n43) * idet;

    ret[3][0] = t14 * idet;
    ret[3][1] = (n13 * n24 * n31 - n14 * n23 * n31 + n14 * n21 * n33 - n11 * n24 * n33 - n13 * n21 * n34 + n11 * n23 * n34) * idet;
    ret[3][2] = (n14 * n22 * n31 - n12 * n24 * n31 - n14 * n21 * n32 + n11 * n24 * n32 + n12 * n21 * n34 - n11 * n22 * n34) * idet;
    ret[3][3] = (n12 * n23 * n31 - n13 * n22 * n31 + n13 * n21 * n32 - n11 * n23 * n32 - n12 * n21 * n33 + n11 * n22 * n33) * idet;

    return ret;
}

void clayxelVertNormalBlend(uint vId, float splatSizeMult, float normalOrientedSplat, out float4 tex, out float3 vertexColor, out float3 outVertPos, out float3 outNormal){
#ifdef CLAYXELS_VALID
	// first we unpack the clayxels point cloud
	int pointId = vId / 3;

	int chunkId = clayxelGetChunkId(pointId);

	int2 clayxelPointData = chunkPoints[pointId];
	
	int4 data1 = unpackInt4(clayxelPointData.x);
	int4 data2;
	int data3;
	unpack66668(clayxelPointData.y, data2, data3);

	float3 normal = unpackNormal2Byte(data1.w, data3);

	float cellSize = chunkSize / 256.0;
	float halfCell = cellSize * 0.5;

	float rough = frac(sin(dot(float2(data1.x,data1.y),float2(12.9898,78.233+data1.z)))*43758.5453123) - 0.5;
	
	float normalRough = rough * _RoughBump;

	float normalOffset = ((((data2.x / 64.0) * 4.0) - 1.0) * halfCell) + normalRough;

	float3 chunkCenter = chunksCenter[chunkId];
	
	float3 cellOffset = float3(cellSize*0.5, cellSize*0.5, cellSize*0.5) + (normal * normalOffset);
	float3 pointPos = expandGridPoint(data1.xyz, cellSize, chunkSize) + cellOffset + chunkCenter;
	float3 p = mul(objectMatrix, float4(pointPos, 1.0)).xyz;
	
	float colorRough = (rough * 0.1) * _RoughColor;
	vertexColor = float3((float)data2.y / 64.0, (float)data2.z / 64.0, (float)data2.w / 64.0) + colorRough;

	outNormal = mul((float3x3)objectMatrix, normal);

	if(solidHighlightId == -2){
		vertexColor += 1.0;
	}
	else if(solidHighlightId > -1){
		int solidId = pointCloudDataToSolidId[pointId];
		if(solidId == solidHighlightId + 1){
			vertexColor += 1.0;
		}
	}

	float newSplatSize = splatRadius * splatSizeMult * 0.9;
	float3 camUpVec = UNITY_MATRIX_V._m10_m11_m12;
	float3 camSideVec = UNITY_MATRIX_V._m00_m01_m02;
	camUpVec += camSideVec * 0.01;//slight offset to avoid invalid cross products with world aligned normals
	
	float3 upVec;
	float3 sideVec;
	
	#if defined(SHADERPASS_SHADOWCASTER) // on shadowPass force splats orientating to normals to prevent holes in the shadows
		sideVec = normalize(cross(camUpVec, outNormal)) * (newSplatSize * 2.0);
		upVec = normalize(cross(sideVec, outNormal)) * newSplatSize;
	#else
		float normalOrientRough = rough * _RoughOrient;
		normalOrientedSplat += normalOrientRough;

		float3 normalSideVec = normalize(cross(outNormal, camUpVec));
		float3 normalUpVec = normalize(cross(normalSideVec, outNormal));

		float orthoPerspective = unity_OrthoParams.w;// pespective = 0.0 , ortho = 1.0
		normalOrientedSplat = lerp(normalOrientedSplat, 0.0, orthoPerspective);// force camera-facing splats on ortho
		
		upVec = normalize(lerp(camUpVec, normalUpVec, normalOrientedSplat)) * (newSplatSize);
		sideVec = normalize(lerp(camSideVec, normalSideVec, normalOrientedSplat)) * (newSplatSize * 2.0);
	#endif

	// expand splat from point P to a triangle with uv coordinates
	uint vertexOffset = vId % 3;
	outVertPos = p + ((upVec * vOffsetUpTable[vertexOffset]) + (sideVec * vOffsetSideTable[vertexOffset]));
	tex = vTexTable[vertexOffset];

	#if !defined(SHADERPASS_SHADOWCASTER) 
		float3 eyeVec = normalize(_WorldSpaceCameraPos - p);
		outVertPos = outVertPos - ((eyeVec * (dot(eyeVec, outVertPos - p))) * (1.0 - orthoPerspective));
	#endif
#else
	tex = float4(0, 0, 0, 0);
	vertexColor = float3(0, 0, 0);
	outVertPos = float3(0, 0, 0);
	outNormal = float3(0, 0, 0);
#endif
}

// keeping this for compatibility with old code, this function is no longer
void clayxelVertFoliage(uint vId, float splatSizeMult, float normalOrientedSplat, out float4 tex, out float3 vertexColor, out float3 outVertPos, out float3 outNormal){
	clayxelVertNormalBlend(vId, splatSizeMult, normalOrientedSplat, tex, vertexColor, outVertPos, outNormal);
}
