
Shader "Clayxels/ClayxelPickingShader" {
	SubShader {
		Tags { "Queue" = "Geometry" "RenderType"="Opaque" }

		Pass {
			Lighting Off

			ZWrite On     
			
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "clayxelSRPUtils.cginc"

			int selectMode = 0;
			int containerId = 0;

			struct VertexData{
				float4 pos: POSITION;
				float4 tex: TEXCOORD0;
				nointerpolation float2 pickId: TEXCOORD1;
			};

			struct FragData{
				fixed4 selection: SV_TARGET;
			};


			VertexData vert(uint id : SV_VertexID){
				// init a blank vertex, we might discard it before to shade it
				VertexData outVertex;
				outVertex.pos = float4(0, 0, 0, 0);
				outVertex.tex = float4(0, 0, 0, 0);
				outVertex.pickId = float2(0, 0);
				
				int pointId = id / 3;

				int chunkId = clayxelGetChunkId(pointId);
				
				float3 chunkCenter = chunksCenter[chunkId];

				if(selectMode == 1){// select clayObject
					outVertex.pickId.x = pointCloudDataToSolidId[pointId];
				}
				
				outVertex.pickId.y = float(containerId) / 255.0;
				
				int2 clayxelPointData = chunkPoints[pointId];
				int4 data1 = unpackInt4(clayxelPointData.x);
				int4 data2;
				int data3;
				unpack66668(clayxelPointData.y, data2, data3);

				float3 normal = unpackNormal2Byte(data1.w, data3);

				float cellSize = chunkSize / 256.0;
				float halfCell = cellSize * 0.5;

				float normalOffset = (((data2.x / 64.0) * 2.0) - 1.0) * halfCell;

				float3 cellOffset = float3(cellSize*0.5, cellSize*0.5, cellSize*0.5) + (normal * normalOffset);
				float3 pointPos = expandGridPoint(data1.xyz, cellSize, chunkSize) + cellOffset + chunkCenter;
				float3 p = mul(objectMatrix, float4(pointPos, 1.0)).xyz;

				// expand verts to billboard
				uint vertexOffset = id % 3;
				float3 upVec = float3(unity_CameraToWorld[0][1], unity_CameraToWorld[1][1], unity_CameraToWorld[2][1]) * splatRadius;
				float3 sideVec = float3(unity_CameraToWorld[0][0], unity_CameraToWorld[1][0], unity_CameraToWorld[2][0]) * (splatRadius * 2.0);

				if(vertexOffset == 0){
					outVertex.pos = UnityObjectToClipPos(float4(p + ((-upVec) + sideVec), 1.0));
					outVertex.tex.xy = float2(-0.5, 0.0);
				}
				else if(vertexOffset == 1){
					outVertex.pos = UnityObjectToClipPos(float4(p + ((-upVec) - sideVec), 1.0));
					outVertex.tex.xy = float2(1.5, 0.0);
				}
				else if(vertexOffset == 2){
					outVertex.pos = UnityObjectToClipPos(float4(p + (upVec*1.7), 1.0));
					outVertex.tex.xy = float2(0.5, 1.35);
				}

				return outVertex;
			}

			FragData frag(VertexData inVertex){
				FragData outData;
				outData.selection = float4(unpackRgb(uint(inVertex.pickId.x)), inVertex.pickId.y);
				
				return outData;
			}

			ENDCG
		}
	}
}