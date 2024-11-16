Shader "Custom/VirtualTexture"
{
    Properties
    {
    }
    SubShader
    {
        Pass
        {
            Tags
            {
                "LightMode" = "VirtualTextureFeedback"
            }
            
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0

            #include "Common.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4x4 unity_ObjectToWorld;
            float4x4 unity_MatrixVP;
            float4 _VTFeedbackParam;
            RWStructuredBuffer<int> _FeedbackBuffer : register(u1);

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = mul(unity_MatrixVP, mul(unity_ObjectToWorld, float4(IN.positionOS, 1.0)));
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
             //    float2 page = floor(IN.uv * _VTFeedbackParam.x);
	            // float2 uv = IN.uv * _VTFeedbackParam.y;
	            // float2 dx = ddx(uv);
	            // float2 dy = ddy(uv);
	            // float mip = clamp(0.5 * log2(max(dot(dx, dx), dot(dy, dy))), _VTFeedbackParam.w, _VTFeedbackParam.z);
             //    mip = floor(mip);
             //    page = floor(page / exp2(mip));
             //    int col = int(page.x);
             //    int row = int(page.y);
                float pageSize = _VTFeedbackParam.x;
                float pageResolution = _VTFeedbackParam.y;
                float maxMipmapLevel = _VTFeedbackParam.z;
                float minMipmapLevel = _VTFeedbackParam.w;
                int3 page = calcPage(IN.uv, pageSize, pageResolution, maxMipmapLevel, minMipmapLevel);
                int row = page.x;
                int col = page.y;
                int mip = page.z;
                int idx = col + row * pageSize + mip * pageSize * pageSize;
                _FeedbackBuffer[idx] = 1;
	            return float4(float2(col, row) / pageSize, mip / maxMipmapLevel, 1);
            }
            
            ENDHLSL
        }
        
        Pass
        {
            Tags
            {
                "LightMode" = "SRPDefaultUnlit"
            }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0

            #include "Common.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4x4 unity_ObjectToWorld;
            float4x4 unity_MatrixVP;
            float4 _VTFeedbackParam;
            float4 _PhyTextureParam;
            Texture2D _PageTable;
            // SamplerState sampler_PageTable;
            Texture2D _PhysicalTexture;
            SamplerState my_point_clamp_sampler;
            // SamplerState sampler_PhysicalTexture;
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = mul(unity_MatrixVP, mul(unity_ObjectToWorld, float4(IN.positionOS, 1.0)));
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float pageSize = _VTFeedbackParam.x;
                float pageResolution = _VTFeedbackParam.y;
                float maxMipmapLevel = _VTFeedbackParam.z;
                float minMipmapLevel = _VTFeedbackParam.w;
                int3 page = calcPage(IN.uv, pageSize, pageResolution, maxMipmapLevel, minMipmapLevel);
                int row = page.x;
                int col = page.y;
                int mip = page.z;
                pageSize = floor(pageSize / exp2(mip));
                // float4 pte = _PageTable.SampleLevel(my_point_clamp_sampler, float2(col, row) / pageSize, mip);
                float4 pte = _PageTable.SampleLevel(my_point_clamp_sampler, IN.uv, mip);
                if (pte.w == 0.0f) return float4(1.0, 0.0, 1.0, 1.0);
                // float2 offsetCoord = (IN.uv * pageSize - float2(col, row)) / _PhyTextureParam.xy;
                float2 offsetCoord = frac(IN.uv * pageSize);
                float2 startCoord = round(pte.xy * _PhyTextureParam.xy);
                float2 coord = (startCoord + offsetCoord) / _PhyTextureParam.xy;
                return float4(pte.xy * _PhyTextureParam.xy, 0.0, 1.0);
                // return float4(offsetCoord * _PhyTextureParam.yx, 0.0, 1.0);
                // return float4(offsetCoord, 0.0, 1.0);
                // return float4(coord, 0.0f, 1.0);
                return pte;
                return _PhysicalTexture.SampleLevel(my_point_clamp_sampler, coord, 0);
            }
            ENDHLSL
        }
    }
}