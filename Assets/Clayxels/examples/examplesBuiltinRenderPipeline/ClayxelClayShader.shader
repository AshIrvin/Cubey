// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ClayxelClayShader"
{
	Properties
	{
		_Metallic("Metallic", Float) = 0
		_Cutoff( "Mask Clip Value", Float ) = 0.9
		_ClayxelSize("ClayxelSize", Float) = 0
		_Smoothness1("Smoothness", Float) = 0
		[NoScaleOffset]_MainTex1("Texture", 2D) = "white" {}
		[HDR]_Emission("Emission", Color) = (1,1,1,1)
		[NoScaleOffset]_normal("normal", 2D) = "white" {}
		_normalStrength("normalStrength", Float) = 0
		_normalTileX("normalTileX", Range( 0 , 10)) = 1
		_normalTileY("normalTileY", Range( 0 , 10)) = 1
		_normalMapOffset("normalMapOffset", Vector) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "AlphaTest+0" "IsEmissive" = "true"  }
		Cull Back
		AlphaToMask On
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 4.5
		#define ASE_TEXTURE_PARAMS(textureName) textureName

		#include "Assets/Clayxels/Resources/clayxelSRPUtils.cginc"
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif

		struct appdata_full_custom
		{
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
			float4 texcoord : TEXCOORD0;
			float4 texcoord1 : TEXCOORD1;
			float4 texcoord2 : TEXCOORD2;
			float4 texcoord3 : TEXCOORD3;
			fixed4 color : COLOR;
			UNITY_VERTEX_INPUT_INSTANCE_ID
			uint ase_vertexId : SV_VertexID;
		};
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
			float4 vertexColor : COLOR;
			float2 uv_texcoord;
		};

		uniform float _ClayxelSize;
		uniform sampler2D _normal;
		uniform float _normalTileX;
		uniform float _normalTileY;
		uniform float3 _normalMapOffset;
		uniform float _normalStrength;
		uniform float4 _Emission;
		uniform float _Metallic;
		uniform float _Smoothness1;
		uniform sampler2D _MainTex1;
		uniform float _Cutoff = 0.9;


		inline float3 TriplanarSamplingSNF( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = ( tex2D( ASE_TEXTURE_PARAMS( topTexMap ), tiling * worldPos.zy * float2( nsign.x, 1.0 ) ) );
			yNorm = ( tex2D( ASE_TEXTURE_PARAMS( topTexMap ), tiling * worldPos.xz * float2( nsign.y, 1.0 ) ) );
			zNorm = ( tex2D( ASE_TEXTURE_PARAMS( topTexMap ), tiling * worldPos.xy * float2( -nsign.z, 1.0 ) ) );
			xNorm.xyz = half3( UnpackScaleNormal( xNorm, normalScale.y ).xy * float2( nsign.x, 1.0 ) + worldNormal.zy, worldNormal.x ).zyx;
			yNorm.xyz = half3( UnpackScaleNormal( yNorm, normalScale.x ).xy * float2( nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
			zNorm.xyz = half3( UnpackScaleNormal( zNorm, normalScale.y ).xy * float2( -nsign.z, 1.0 ) + worldNormal.xy, worldNormal.z ).xyz;
			return normalize( xNorm.xyz * projNormal.x + yNorm.xyz * projNormal.y + zNorm.xyz * projNormal.z );
		}


		void vertexDataFunc( inout appdata_full_custom v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float localclayxelComputeVertex7 = ( 0.0 );
			int vertexId7 = v.ase_vertexId;
			float3 vertexPosition7 = float3( 0,0,0 );
			float3 vertexNormal7 = float3( 0,0,0 );
			float clayxelSize7 = _ClayxelSize;
			float normalOrient7 = 1.0;
			clayxelVertNormalBlend(vertexId7 , clayxelSize7, normalOrient7, v.texcoord, v.color.xyz, vertexPosition7, vertexNormal7); 
			v.vertex.w = 1.0;
			v.tangent = float4(normalize(cross(UNITY_MATRIX_V._m20_m21_m22, vertexNormal7)),0.5);
			v.vertex.xyz = vertexPosition7;
			v.normal = vertexNormal7;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 appendResult142 = (float2(_normalTileX , _normalTileY));
			float localclayxelsWorldToObject173 = ( 0.0 );
			// UNITY HACK/FIX: for some reason this matrix is identity, we need to set it back so that triplanar can work in object space
			unity_WorldToObject = inverse(objectMatrix);
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_worldToTangent = float3x3( ase_worldTangent, ase_worldBitangent, ase_worldNormal );
			float4 ase_vertexTangent = mul( unity_WorldToObject, float4( ase_worldTangent, 0 ) );
			float3 ase_vertexBitangent = mul( unity_WorldToObject, float4( ase_worldBitangent, 0 ) );
			float3 ase_vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
			float3x3 objectToTangent = float3x3(ase_vertexTangent.xyz, ase_vertexBitangent, ase_vertexNormal);
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 triplanar100 = TriplanarSamplingSNF( _normal, mul( float4( ( ase_vertex3Pos + _normalMapOffset ) , 0.0 ), UNITY_MATRIX_M ).xyz, ase_vertexNormal, ( 1.0 + localclayxelsWorldToObject173 ), appendResult142, _normalStrength, 0 );
			float3 tanTriplanarNormal100 = mul( objectToTangent, triplanar100 );
			o.Normal = tanTriplanarNormal100;
			o.Albedo = i.vertexColor.rgb;
			o.Emission = _Emission.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness1;
			o.Alpha = 1;
			clip( tex2D( _MainTex1, i.uv_texcoord ).a - _Cutoff );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows nodynlightmap vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			AlphaToMask Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.5
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				half4 color : COLOR0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full_custom v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.color = v.color;
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				surfIN.vertexColor = IN.color;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=18000
269;281;1413;628;2563.895;696.8419;1.6;True;False
Node;AmplifyShaderEditor.PosVertexDataNode;148;-2211.157,-66.06409;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;134;-2195.797,133.3394;Inherit;False;Property;_normalMapOffset;normalMapOffset;10;0;Create;True;0;0;False;0;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.MMatrixNode;169;-1830.2,292.1093;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;166;-1831.863,58.82207;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CustomExpressionNode;173;-1807.61,-61.56998;Inherit;False;// UNITY HACK/FIX: for some reason this matrix is identity, we need to set it back so that triplanar can work in object space$unity_WorldToObject = inverse(objectMatrix)@$;7;False;0;clayxelsWorldToObject;False;False;0;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;178;-1789.495,-151.2421;Inherit;False;Constant;_falloff;falloff;11;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;143;-1931.44,-320.7541;Inherit;False;Property;_normalTileY;normalTileY;9;0;Create;True;0;0;False;0;1;0;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;141;-1924.622,-409.3932;Inherit;False;Property;_normalTileX;normalTileX;8;0;Create;True;0;0;False;0;1;0;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;119;-676.8085,572.579;Inherit;False;Constant;_orient;orient;9;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;2;-806.6102,448.4052;Inherit;False;Property;_ClayxelSize;ClayxelSize;2;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexIdVariableNode;5;-606.8448,369.614;Inherit;False;0;1;INT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;4;-1418.546,180.2328;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;179;-1491.895,-183.2421;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;104;-1827.57,-242.0164;Inherit;False;Property;_normalStrength;normalStrength;7;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;142;-1608.33,-355.3691;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;102;-1606.038,-601.951;Inherit;True;Property;_normal;normal;6;1;[NoScaleOffset];Create;True;0;0;False;0;None;07944ba93895c3d43869c3b14beb60b4;False;white;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;167;-1689.859,207.9051;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexturePropertyNode;3;-1384.427,-18.78528;Inherit;True;Property;_MainTex1;Texture;4;1;[NoScaleOffset];Create;False;0;0;False;0;None;1aa3096b1b9d9204eaa6c75a4275adb1;False;white;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TriplanarNode;100;-1186.048,-312.2373;Inherit;True;Spherical;Object;True;Top Texture 0;_TopTexture0;white;0;None;Mid Texture 0;_MidTexture0;white;0;None;Bot Texture 0;_BotTexture0;white;1;None;Triplanar Sampler;False;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;12;-432.2874,-93.2849;Inherit;False;Property;_Emission;Emission;5;1;[HDR];Create;True;0;0;False;0;1,1,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;10;-695.9645,-23.20914;Inherit;False;Property;_Metallic;Metallic;0;0;Create;True;0;0;True;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-694.7849,72.86772;Inherit;False;Property;_Smoothness1;Smoothness;3;0;Create;False;0;0;True;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;7;-381.56,351.0013;Inherit;False;$clayxelVertNormalBlend(vertexId , clayxelSize, normalOrient, v.texcoord, v.color.xyz, vertexPosition, vertexNormal)@ $v.vertex.w = 1.0@$$v.tangent = float4(normalize(cross(UNITY_MATRIX_V._m20_m21_m22, vertexNormal)),0.5)@$;7;True;5;False;vertexId;INT;0;In;;Inherit;False;False;vertexPosition;FLOAT3;0,0,0;Out;;Inherit;False;False;vertexNormal;FLOAT3;0,0,0;Out;;Inherit;False;False;clayxelSize;FLOAT;0;In;;Inherit;False;False;normalOrient;FLOAT;0;In;;Inherit;False;clayxelComputeVertex;False;False;0;6;0;FLOAT;0;False;1;INT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;3;FLOAT;0;FLOAT3;3;FLOAT3;4
Node;AmplifyShaderEditor.VertexColorNode;6;-710.5934,-485.635;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;11;-1039.756,76.75246;Inherit;True;Property;_TextureSample0;Texture Sample 0;6;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;77;174.7608,-24.62549;Float;False;True;-1;5;;0;0;Standard;ClayxelClayShader;False;False;False;False;False;False;False;True;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;4;Custom;0.9;True;True;0;True;Opaque;;AlphaTest;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;5;False;-1;10;False;-1;0;5;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;5.8;0,0,0,0;VertexScale;True;False;Cylindrical;False;Absolute;0;;1;-1;-1;-1;0;True;0;0;False;-1;-1;0;False;-1;1;Include;Assets/Clayxels/Resources/clayxelSRPUtils.cginc;False;;Custom;0;0;False;0.1;False;-1;0;False;9;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;166;0;148;0
WireConnection;166;1;134;0
WireConnection;179;0;178;0
WireConnection;179;1;173;0
WireConnection;142;0;141;0
WireConnection;142;1;143;0
WireConnection;167;0;166;0
WireConnection;167;1;169;0
WireConnection;100;0;102;0
WireConnection;100;9;167;0
WireConnection;100;8;104;0
WireConnection;100;3;142;0
WireConnection;100;4;179;0
WireConnection;7;1;5;0
WireConnection;7;4;2;0
WireConnection;7;5;119;0
WireConnection;11;0;3;0
WireConnection;11;1;4;0
WireConnection;77;0;6;0
WireConnection;77;1;100;0
WireConnection;77;2;12;0
WireConnection;77;3;10;0
WireConnection;77;4;8;0
WireConnection;77;10;11;4
WireConnection;77;11;7;3
WireConnection;77;12;7;4
ASEEND*/
//CHKSM=605BC7A1FCC073A9C28ED275725AB53137814927