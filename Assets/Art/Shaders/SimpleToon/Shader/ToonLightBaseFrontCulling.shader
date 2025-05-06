Shader "Lpk/LightModel/ToonGlassInside"
{
    Properties
    {
        _BaseMap            ("Texture", 2D)                       = "white" {}
        _BaseColor          ("Color", Color)                      = (0.5,0.5,0.5,1)

        [Space]
        _ShadowStep         ("ShadowStep", Range(0, 1))           = 0.5
        _ShadowStepSmooth   ("ShadowStepSmooth", Range(0, 1))     = 0.04

        [Space]
        _Transparency       ("Transparency", Range(0.0, 1.0))     = 0.2

        [Space]   
        _OutlineWidth       ("OutlineWidth", Range(0.0, 1.0))      = 0.15
        _OutlineColor       ("OutlineColor", Color)               = (0.0, 0.0, 0.0, 1)
        _OutlineAlpha       ("Outline Alpha", Range(0.0, 1.0))     = 1.0
    }

    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "RenderPipeline" = "UniversalPipeline" 
        }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            Cull Off

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _ShadowStep;
                float _ShadowStepSmooth;
                float _Transparency;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv            : TEXCOORD0;
                float4 normalWS      : TEXCOORD1;
                float4 tangentWS     : TEXCOORD2;
                float4 bitangentWS   : TEXCOORD3;
                float3 viewDirWS     : TEXCOORD4;
                float4 shadowCoord   : TEXCOORD5;
                float4 fogCoord      : TEXCOORD6;
                float3 positionWS    : TEXCOORD7;
                float4 positionCS    : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                float3 viewDirWS = GetCameraPositionWS() - vertexInput.positionWS;

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.uv = input.uv;
                output.normalWS = float4(normalInput.normalWS, 0);
                output.tangentWS = float4(normalInput.tangentWS, 0);
                output.bitangentWS = float4(normalInput.bitangentWS, 0);
                output.viewDirWS = viewDirWS;
                output.shadowCoord = TransformWorldToShadowCoord(vertexInput.positionWS);
                output.fogCoord = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float2 uv = input.uv;
                float3 V = normalize(input.viewDirWS.xyz);
                float3 N = normalize(input.normalWS.xyz);

                if (dot(N, V) > 0) N = -N;

                float3 L = normalize(_MainLightPosition.xyz);
                float NL = dot(N, L);
                NL = NL * 0.5 + 0.5;

                float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);

                float shadowNL = smoothstep(_ShadowStep - _ShadowStepSmooth, _ShadowStep + _ShadowStepSmooth, NL);
                float shadow = MainLightRealtimeShadow(input.shadowCoord);

                float3 diffuse = _MainLightColor.rgb * baseMap.rgb * _BaseColor.rgb * shadowNL * shadow;
                float3 ambient = SampleSH(N) * _BaseColor.rgb * baseMap.rgb;

                float3 finalColor = diffuse + ambient;
                finalColor = MixFog(finalColor, input.fogCoord);

                return float4(finalColor, _Transparency);
            }
            ENDHLSL
        }

        // Outline Pass with transparency
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Off
            ZWrite On
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 fogCoord : TEXCOORD0;
            };

            float _OutlineWidth;
            float4 _OutlineColor;
            float _OutlineAlpha;

            v2f vert(appdata v)
            {
                v2f o;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
                o.pos = TransformObjectToHClip(float4(v.vertex.xyz + v.normal * _OutlineWidth * 0.1, 1));
                o.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 finalColor = MixFog(_OutlineColor.rgb, i.fogCoord);
                return float4(finalColor, _OutlineAlpha);
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}
