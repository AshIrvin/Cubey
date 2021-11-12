
Shader "Clayxels/ClayxelMicroVoxelPassURP"
{
    Properties
    {
        _backFillDark("Backdrop Darkner", Range(0.0, 1.0)) = 0.0
        _alphaCutout("Splat Cutout", Range(0.0, 1.0)) = 0.0
        _splatSizeMult("Splat Size", Range(0.0, 1.0)) = 0.0
        _roughSize("Rough Size", Range(0.0, 1.0)) = 0.0
        _roughColor("Rough Color", Range(0.0, 1.0)) = 0.0
        _roughPos("Rough Position", Range(0.0, 1.0)) = 0.0
        _roughTwist("Rough Twist", Range(0.0, 10.0)) = 0.0
        _roughOrientX("Rough Orient X", Range(-1.0, 1.0)) = 0.0
        _roughOrientY("Rough Orient Y", Range(-1.0, 1.0)) = 0.0
        _roughOrientZ("Rough Orient Z", Range(-1.0, 1.0)) = 0.0
        _roughOffset("Rough Offset", Range(0.0, 1.0)) = 0.0
        [NoScaleOffset]_MainTex("SplatTexture", 2D) = "white" {}

        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 1
    }

    SubShader
    {
        Pass
        {
            Name "FirstPass"
            Tags { "LightMode" = "FirstPass" }

            ZWrite On
            ZTest LEqual
            Cull [_Cull]

            HLSLPROGRAM

            #pragma target 4.5

            #pragma multi_compile CLAYXEL_EARLY_Z_OPTIMIZE_ON CLAYXEL_EARLY_Z_OPTIMIZE_OFF

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"

            #include "../clayxelMicroVoxelUtils.cginc"

            #pragma vertex MicroVoxelPassVert
            #pragma fragment MicroVoxelPassFrag
            

            ENDHLSL
        }
    }
}