// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ClayxelFoliageShader"
{
	Properties
	{
		_Metallic("Metallic", Float) = 0
		_ClayxelSize("ClayxelSize", Float) = 0
		_Fuzz("Fuzz", Float) = 0
		_NormalOrient("NormalOrient", Float) = 0
		_Smoothness1("Smoothness", Float) = 0
		[NoScaleOffset]_MainTex1("Texture", 2D) = "white" {}
		[HDR]_Emission("Emission", Color) = (1,1,1,1)
		_randomize("randomize", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "AlphaTest+0" "IsEmissive" = "true"  }
		Cull Back
		AlphaToMask On
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
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
			float2 vertexToFrag91;
			float2 uv_texcoord;
		};

		uniform float _ClayxelSize;
		uniform float _NormalOrient;
		uniform sampler2D _MainTex1;
		uniform float _randomize;
		uniform float4 _Emission;
		uniform float _Metallic;
		uniform float _Smoothness1;
		uniform float _Fuzz;

		void vertexDataFunc( inout appdata_full_custom v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float localclayxelComputeVertex7 = ( 0.0 );
			int vertexId7 = v.ase_vertexId;
			float3 vertexPosition7 = float3( 0,0,0 );
			float3 vertexNormal7 = float3( 0,0,0 );
			float clayxelSize7 = _ClayxelSize;
			float normalOrient7 = ( _NormalOrient * 0.45 );
			clayxelVertNormalBlend(vertexId7 , clayxelSize7, normalOrient7, v.texcoord, v.color.xyz, vertexPosition7, vertexNormal7); 
			v.vertex.w = 1.0; // fix shadows in builtin renderer
			v.vertex.xyz = vertexPosition7;
			v.normal = vertexNormal7;
			float localclayxelGetPointCloud34 = ( 0.0 );
			int vertexId34 = v.ase_vertexId;
			float3 pointCenter34 = float3( 0,0,0 );
			float3 pointNormal34 = float3( 0,0,0 );
			float3 pointColor34 = float3( 0,0,0 );
			float3 gridPoint34 = float3( 0,0,0 );
			clayxelGetPointCloud(vertexId34, gridPoint34, pointColor34, pointCenter34, pointNormal34);
			float3 break70 = cross( pointCenter34 , pointNormal34 );
			float2 appendResult69 = (float2(break70.x , break70.y));
			float dotResult4_g3 = dot( appendResult69 , float2( 12.9898,78.233 ) );
			float lerpResult10_g3 = lerp( 0.0 , 1000.0 , frac( ( sin( dotResult4_g3 ) * 43758.55 ) ));
			float cos29 = cos( ( lerpResult10_g3 * _randomize ) );
			float sin29 = sin( ( lerpResult10_g3 * _randomize ) );
			float2 rotator29 = mul( v.texcoord.xy - float2( 0.5,0.5 ) , float2x2( cos29 , -sin29 , sin29 , cos29 )) + float2( 0.5,0.5 );
			o.vertexToFrag91 = rotator29;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 tex2DNode11 = tex2D( _MainTex1, i.vertexToFrag91 );
			o.Albedo = ( i.vertexColor * tex2DNode11 ).rgb;
			o.Emission = _Emission.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness1;
			o.Alpha = 1;
			float dotResult4_g4 = dot( i.uv_texcoord , float2( 12.9898,78.233 ) );
			float lerpResult10_g4 = lerp( 0.0 , tex2DNode11.a , frac( ( sin( dotResult4_g4 ) * 43758.55 ) ));
			float ifLocalVar27 = 0;
			UNITY_BRANCH 
			if( tex2DNode11.a > _Fuzz )
				ifLocalVar27 = lerpResult10_g4;
			clip( ifLocalVar27 - _Fuzz );
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
				float4 customPack1 : TEXCOORD1;
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
				o.customPack1.xy = customInputData.vertexToFrag91;
				o.customPack1.zw = customInputData.uv_texcoord;
				o.customPack1.zw = v.texcoord;
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
				surfIN.vertexToFrag91 = IN.customPack1.xy;
				surfIN.uv_texcoord = IN.customPack1.zw;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
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
167;237;1413;628;1143.603;383.9052;1.417243;True;False
Node;AmplifyShaderEditor.VertexIdVariableNode;35;-3236.483,-243.2999;Inherit;False;0;1;INT;0
Node;AmplifyShaderEditor.CustomExpressionNode;34;-3064.402,-193.1563;Inherit;False;clayxelGetPointCloud(vertexId, gridPoint, pointColor, pointCenter, pointNormal)@;7;True;5;False;vertexId;INT;0;In;;Inherit;False;True;pointCenter;FLOAT3;0,0,0;Out;;Inherit;False;True;pointNormal;FLOAT3;0,0,0;Out;;Inherit;False;True;pointColor;FLOAT3;0,0,0;Out;;Inherit;False;True;gridPoint;FLOAT3;0,0,0;Out;;Inherit;False;clayxelGetPointCloud;False;False;0;6;0;FLOAT;0;False;1;INT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;5;FLOAT;0;FLOAT3;3;FLOAT3;4;FLOAT3;5;FLOAT3;6
Node;AmplifyShaderEditor.CrossProductOpNode;90;-2766.241,-121.1199;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;70;-2577.287,-70.86201;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;69;-2268.469,-24.17117;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;50;-2202.092,279.5187;Inherit;False;Property;_randomize;randomize;7;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;31;-2107.71,120.0017;Inherit;False;Random Range;-1;;3;7b754edb8aebbfb4a9ace907af661cfc;0;3;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT;1000;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;4;-1993.451,-58.21102;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;-1907.031,153.5305;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;29;-1742.461,46.0318;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;3;-1556.901,-189.2809;Inherit;True;Property;_MainTex1;Texture;5;1;[NoScaleOffset];Create;False;0;0;False;0;None;1aa3096b1b9d9204eaa6c75a4275adb1;False;white;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.VertexToFragmentNode;91;-1528.983,26.16465;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;92;-1546.697,195.9639;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;1;-926.1871,591.6495;Inherit;False;Property;_NormalOrient;NormalOrient;3;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;11;-1296.131,-167.2464;Inherit;True;Property;_TextureSample0;Texture Sample 0;6;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;9;-1016.258,274.1686;Inherit;False;Property;_Fuzz;Fuzz;2;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;26;-993.85,126.6098;Inherit;False;Random Range;-1;;4;7b754edb8aebbfb4a9ace907af661cfc;0;3;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;6;-598.1531,-448.7143;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;2;-735.8527,498.3518;Inherit;False;Property;_ClayxelSize;ClayxelSize;1;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexIdVariableNode;5;-606.8448,369.614;Inherit;False;0;1;INT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;93;-604.22,640.8062;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.45;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-376.6388,-266.5422;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-695.9645,-23.20914;Inherit;False;Property;_Metallic;Metallic;0;0;Create;True;0;0;True;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;27;-644.673,177.6142;Inherit;False;True;5;0;FLOAT;0;False;1;FLOAT;0.1;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;12;-422.2181,-152.0223;Inherit;False;Property;_Emission;Emission;6;1;[HDR];Create;True;0;0;False;0;1,1,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CustomExpressionNode;7;-381.56,351.0013;Inherit;False;$clayxelVertNormalBlend(vertexId , clayxelSize, normalOrient, v.texcoord, v.color.xyz, vertexPosition, vertexNormal)@ $v.vertex.w = 1.0@ // fix shadows in builtin renderer$$;7;True;5;False;vertexId;INT;0;In;;Inherit;False;False;vertexPosition;FLOAT3;0,0,0;Out;;Inherit;False;False;vertexNormal;FLOAT3;0,0,0;Out;;Inherit;False;False;clayxelSize;FLOAT;0;In;;Inherit;False;False;normalOrient;FLOAT;0;In;;Inherit;False;clayxelComputeVertex;False;False;0;6;0;FLOAT;0;False;1;INT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;3;FLOAT;0;FLOAT3;3;FLOAT3;4
Node;AmplifyShaderEditor.RangedFloatNode;8;-694.7849,72.86772;Inherit;False;Property;_Smoothness1;Smoothness;4;0;Create;False;0;0;True;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;77;174.7608,-24.62549;Float;False;True;-1;2;;0;0;Standard;ClayxelFoliageShader;False;False;False;False;False;False;False;True;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;4;Custom;0.5;True;True;0;True;Opaque;;AlphaTest;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;5;False;-1;10;False;-1;0;5;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;5.8;0,0,0,0;VertexScale;True;False;Cylindrical;False;Absolute;0;;-1;-1;-1;-1;0;True;0;0;False;-1;-1;0;True;9;1;Include;Assets/Clayxels/Resources/clayxelSRPUtils.cginc;False;;Custom;0;0;False;0.1;False;-1;0;False;9;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;34;1;35;0
WireConnection;90;0;34;3
WireConnection;90;1;34;4
WireConnection;70;0;90;0
WireConnection;69;0;70;0
WireConnection;69;1;70;1
WireConnection;31;1;69;0
WireConnection;49;0;31;0
WireConnection;49;1;50;0
WireConnection;29;0;4;0
WireConnection;29;2;49;0
WireConnection;91;0;29;0
WireConnection;11;0;3;0
WireConnection;11;1;91;0
WireConnection;26;1;92;0
WireConnection;26;3;11;4
WireConnection;93;0;1;0
WireConnection;24;0;6;0
WireConnection;24;1;11;0
WireConnection;27;0;11;4
WireConnection;27;1;9;0
WireConnection;27;2;26;0
WireConnection;7;1;5;0
WireConnection;7;4;2;0
WireConnection;7;5;93;0
WireConnection;77;0;24;0
WireConnection;77;2;12;0
WireConnection;77;3;10;0
WireConnection;77;4;8;0
WireConnection;77;10;27;0
WireConnection;77;11;7;3
WireConnection;77;12;7;4
ASEEND*/
//CHKSM=DD999A9B125324BA4590C53C1048CD7F80D06185