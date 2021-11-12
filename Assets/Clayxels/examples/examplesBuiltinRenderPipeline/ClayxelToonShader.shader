// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ClayxelToonShader"
{
	Properties
	{
		_ClayxelSize("ClayxelSize", Range( 0.1 , 1.5)) = 1
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		[NoScaleOffset]_MainTex1("Texture", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" }
		Cull Off
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#include "Assets/Clayxels/Resources/clayxelSRPUtils.cginc"

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
			float4 vertexColor : COLOR;
			float2 uv_texcoord;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform float _ClayxelSize;
		uniform sampler2D _MainTex1;
		uniform float _Cutoff = 0.5;

		void vertexDataFunc( inout appdata_full_custom v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float localclayxelComputeVertex7 = ( 0.0 );
			int vertexId7 = v.ase_vertexId;
			float3 vertexPosition7 = float3( 0,0,0 );
			float3 vertexNormal7 = float3( 0,0,0 );
			float clayxelSize7 = _ClayxelSize;
			float normalOrient7 = 0.0;
			clayxelVertNormalBlend(vertexId7 , clayxelSize7, normalOrient7, v.texcoord, v.color.xyz, vertexPosition7, vertexNormal7); 
			#if defined(SHADERPASS_SHADOWCASTER)
			v.vertex.w = 1.0; // fix shadows in builtin renderer
			#endif
			v.vertex.xyz = vertexPosition7;
			v.normal = vertexNormal7;
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			#ifdef UNITY_PASS_FORWARDBASE
			float ase_lightAtten = data.atten;
			if( _LightColor0.a == 0)
			ase_lightAtten = 0;
			#else
			float3 ase_lightAttenRGB = gi.light.color / ( ( _LightColor0.rgb ) + 0.000001 );
			float ase_lightAtten = max( max( ase_lightAttenRGB.r, ase_lightAttenRGB.g ), ase_lightAttenRGB.b );
			#endif
			#if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
			half bakedAtten = UnitySampleBakedOcclusion(data.lightmapUV.xy, data.worldPos);
			float zDist = dot(_WorldSpaceCameraPos - data.worldPos, UNITY_MATRIX_V[2].xyz);
			float fadeDist = UnityComputeShadowFadeDistance(data.worldPos, zDist);
			ase_lightAtten = UnityMixRealtimeAndBakedShadows(data.atten, bakedAtten, UnityComputeShadowFade(fadeDist));
			#endif
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			c.rgb = ( i.vertexColor * float4( ( ase_lightAtten * ase_lightColor.rgb ) , 0.0 ) ).rgb;
			c.a = 1;
			clip( tex2D( _MainTex1, i.uv_texcoord ).a - _Cutoff );
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			float4 temp_output_6_0 = i.vertexColor;
			o.Albedo = temp_output_6_0.rgb;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
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
				float3 worldPos : TEXCOORD2;
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
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
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
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.vertexColor = IN.color;
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT( UnityGI, gi );
				o.Alpha = LightingStandardCustomLighting( o, worldViewDir, gi ).a;
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
167;237;1413;628;1286.481;-33.9861;1;True;False
Node;AmplifyShaderEditor.LightColorNode;105;-533.1129,62.56911;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.LightAttenuation;104;-551.7554,-46.62129;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexIdVariableNode;5;-525.6176,300.3712;Inherit;False;0;1;INT;0
Node;AmplifyShaderEditor.TexturePropertyNode;3;-1220.66,-54.51107;Inherit;True;Property;_MainTex1;Texture;2;1;[NoScaleOffset];Create;False;0;0;False;0;1aa3096b1b9d9204eaa6c75a4275adb1;b9385d232bb3338469705512dca8a12a;False;white;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;4;-1199.223,196.2176;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;106;-314.7321,13.30023;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.VertexColorNode;6;-398.9502,-333.7456;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;2;-666.61,397.1509;Inherit;False;Property;_ClayxelSize;ClayxelSize;0;0;Create;True;0;0;False;0;1;2.21;0.1;1.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1;-667.3044,482.5592;Inherit;False;Constant;_NormalOrient;NormalOrient;2;0;Create;True;0;0;False;0;0;0.3;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;107;-39.88856,-72.03152;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;11;-860.5441,97.21779;Inherit;True;Property;_TextureSample0;Texture Sample 0;6;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CustomExpressionNode;7;-311.9438,327.513;Inherit;False;clayxelVertNormalBlend(vertexId , clayxelSize, normalOrient, v.texcoord, v.color.xyz, vertexPosition, vertexNormal)@ $#if defined(SHADERPASS_SHADOWCASTER)$v.vertex.w = 1.0@ // fix shadows in builtin renderer$#endif;7;True;5;False;vertexId;INT;0;In;;Inherit;False;False;vertexPosition;FLOAT3;0,0,0;Out;;Inherit;False;False;vertexNormal;FLOAT3;0,0,0;Out;;Inherit;False;False;clayxelSize;FLOAT;0;In;;Inherit;False;False;normalOrient;FLOAT;0;In;;Inherit;False;clayxelComputeVertex;False;False;0;6;0;FLOAT;0;False;1;INT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;3;FLOAT;0;FLOAT3;3;FLOAT3;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;101;409.5154,-255.2251;Float;False;True;-1;2;;0;0;CustomLighting;ClayxelToonShader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Absolute;0;;1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;1;Include;Assets/Clayxels/Resources/clayxelSRPUtils.cginc;False;;Custom;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;106;0;104;0
WireConnection;106;1;105;1
WireConnection;107;0;6;0
WireConnection;107;1;106;0
WireConnection;11;0;3;0
WireConnection;11;1;4;0
WireConnection;7;1;5;0
WireConnection;7;4;2;0
WireConnection;7;5;1;0
WireConnection;101;0;6;0
WireConnection;101;10;11;4
WireConnection;101;13;107;0
WireConnection;101;11;7;3
WireConnection;101;12;7;4
ASEEND*/
//CHKSM=816D84A38B2210110847F96CBB36B7A23A407A7B