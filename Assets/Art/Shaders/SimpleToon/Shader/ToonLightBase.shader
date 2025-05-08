Shader "Lpk/LightModel/ToonLightBase"
{
    Properties
    {
        _BaseMap("Texture", 2D) = "white" {}
        _BaseColor("Color", Color) = (0.5,0.5,0.5,1)

        [Normal]_NormalMap("Normal Map", 2D) = "bump" {}

        [Space]
        _ShadowStep("ShadowStep", Range(0, 1)) = 0.5
        _ShadowStepSmooth("ShadowStepSmooth", Range(0, 1)) = 0.04

        [Space]
        _SpecularStep("SpecularStep", Range(0, 1)) = 0.6
        _SpecularStepSmooth("SpecularStepSmooth", Range(0, 1)) = 0.05
        [HDR]_SpecularColor("SpecularColor", Color) = (1,1,1,1)

        [Space]
        _RimStep("RimStep", Range(0, 1)) = 0.65
        _RimStepSmooth("RimStepSmooth", Range(0,1)) = 0.4
        _RimColor("RimColor", Color) = (1,1,1,1)

        [Space]
        _OutlineWidth("OutlineWidth", Range(0.0, 1.0)) = 0.15
        _OutlineColor("OutlineColor", Color) = (0.0, 0.0, 0.0, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

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
            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _ShadowStep;
                float _ShadowStepSmooth;
                float _SpecularStep;
                float _SpecularStepSmooth;
                float4 _SpecularColor;
                float _RimStepSmooth;
                float _RimStep;
                float4 _RimColor;
                float _OutlineWidth;
                float4 _OutlineColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 normalWS : TEXCOORD1;
                float4 tangentWS : TEXCOORD2;
                float4 bitangentWS : TEXCOORD3;
                float3 viewDirWS : TEXCOORD4;
                float4 shadowCoord : TEXCOORD5;
                float4 fogCoord : TEXCOORD6;
                float3 positionWS : TEXCOORD7;
                float4 positionCS : SV_POSITION;
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
                output.normalWS = float4(normalInput.normalWS, viewDirWS.x);
                output.tangentWS = float4(normalInput.tangentWS, viewDirWS.y);
                output.bitangentWS = float4(normalInput.bitangentWS, viewDirWS.z);
                output.viewDirWS = viewDirWS;
                output.shadowCoord = TransformWorldToShadowCoord(vertexInput.positionWS);
                output.fogCoord = ComputeFogFactor(output.positionCS.z);

                return output;
            }

            half remap(half x, half t1, half t2, half s1, half s2)
            {
                return (x - t1) / (t2 - t1) * (s2 - s1) + s1;
            }

            float4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float2 uv = input.uv;

                // Sample and unpack normal map (Tangent Space)
                float3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv));

                // Construct TBN matrix
                float3 T = normalize(input.tangentWS.xyz);
                float3 B = normalize(input.bitangentWS.xyz);
                float3 N_geom = normalize(input.normalWS.xyz);
                float3x3 TBN = float3x3(T, B, N_geom);

                float3 N = normalize(mul(normalTS, TBN)); // Transformed normal

                float3 V = normalize(input.viewDirWS);
                float3 L = normalize(_MainLightPosition.xyz);
                float3 H = normalize(V + L);

                float NV = dot(N, V);
                float NH = dot(N, H);
                float NL = dot(N, L);

                NL = NL * 0.5 + 0.5;

                float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);

                float specularNH = smoothstep((1 - _SpecularStep * 0.05) - _SpecularStepSmooth * 0.05,
                                              (1 - _SpecularStep * 0.05) + _SpecularStepSmooth * 0.05, NH);

                float shadowNL = smoothstep(_ShadowStep - _ShadowStepSmooth, _ShadowStep + _ShadowStepSmooth, NL);
                float shadow = MainLightRealtimeShadow(input.shadowCoord);

                float rim = smoothstep((1 - _RimStep) - _RimStepSmooth * 0.5,
                                       (1 - _RimStep) + _RimStepSmooth * 0.5, 0.5 - NV);

                float3 diffuse = _MainLightColor.rgb * baseMap.rgb * _BaseColor.rgb * shadowNL * shadow;
                float3 specular = _SpecularColor.rgb * shadow * shadowNL * specularNH;
                float3 ambient = rim * _RimColor.rgb + SampleSH(N) * _BaseColor.rgb * baseMap.rgb;

                float3 finalColor = diffuse + ambient + specular;
                finalColor = MixFog(finalColor, input.fogCoord);

                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }

        // Outline Pass
        Pass
        {
            Name "Outline"
            Cull Front
            Tags { "LightMode" = "SRPDefaultUnlit" }

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

            v2f vert(appdata v)
            {
                v2f o;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
                float3 norm = normalize(v.normal);
                float3 offset = norm * _OutlineWidth * 0.1;
                float3 displaced = v.vertex.xyz + offset;
                o.pos = TransformObjectToHClip(float4(displaced, 1));
                o.fogCoord = ComputeFogFactor(o.pos.z);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 color = MixFog(_OutlineColor.rgb, i.fogCoord);
                return float4(color, _OutlineColor.a);
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}
