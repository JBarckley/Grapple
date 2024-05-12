Shader "Custom/Quantization" {


    Properties 
    {
        [MainTexture] _QuantTex ("Texture", 2D) = "white"
    }

    SubShader {

        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass {
            HLSLPROGRAM
            #pragma vertex vp
            #pragma fragment fp

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct VertexData {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vp(VertexData v) {
                v2f vf;
                VertexPositionInputs positions = GetVertexPositionInputs(v.vertex.xyz);

                vf.vertex = positions.positionCS;
                vf.uv = v.uv;
                return vf;
            }

            TEXTURE2D(_QuantTex);
            SAMPLER(point_clamp_sampler);

            half4 fp(v2f i) : SV_Target {
                //float4 col = _QuantTex.Sample(point_clamp_sampler, i.uv);
                half4 col = SAMPLE_TEXTURE2D(_QuantTex, point_clamp_sampler, i.uv);

                return half4(1, 0, 1, 1);
            }
            ENDHLSL
        }
    }
}