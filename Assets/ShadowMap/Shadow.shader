Shader "Custom/Shadow"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (1, 1, 1, 1)
        _DepthBias ("Depth Bias", Float) = 0.005
    }
    
    SubShader
    {
        Pass
        {
            Tags
            {
                "LightMode" = "CasterShadow"
            }
            
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            
            float4x4 unity_ObjectToWorld;
            float4x4 unity_MatrixVP;

            float4 vert(float4 positionOS: POSITION): SV_POSITION
            {
                return mul(unity_MatrixVP, mul(unity_ObjectToWorld, positionOS));
            }

            float4 frag(): SV_Target
            {
                return float4(0.0, 0.0, 0.0, 0.0);
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

            struct Attributes
            {
                float4 positionOS: POSITION;
                float3 normalOS: NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS: SV_POSITION;
                float3 normalWS: TEXCOORD0;
                float4 positionLS: TEXCOORD1;
            };

            float4x4 unity_ObjectToWorld;
            float4x4 unity_WorldToObject;
            float4x4 unity_MatrixVP;
            float4 _MainColor;
            float3 _LightDir;
            float4x4 _LightMatrix;
            sampler2D _ShadowMap;
            float _DepthBias;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                const float4 positionWS = mul(unity_ObjectToWorld, IN.positionOS);
                OUT.positionHCS = mul(unity_MatrixVP, positionWS);
                OUT.normalWS = mul(IN.normalOS, (float3x3)unity_WorldToObject);
                OUT.positionLS = mul(_LightMatrix, positionWS);
                return OUT;
            }

            float4 frag(Varyings IN): SV_Target
            {
                float3 ndc = IN.positionLS.xyz / IN.positionLS.w; // xy in [-1, 1] z in [1, 0] (reversed-z)
                float2 uv = ndc.xy * 0.5 + 0.5;
                uv.y = 1.0 - uv.y;
                const float closestDepth = tex2D(_ShadowMap, uv).r;
                const float currDepth = ndc.z;
                const float bias = max(0.05 * (1.0 - dot(normalize(-_LightDir), normalize(IN.normalWS))), _DepthBias); // learnopengl
                float shadow = currDepth + bias < closestDepth ? 1.0 : 0.0; // reversed-z
                if (uv.x < 0.0 || uv.x > 1.0) shadow = 1.0;
                if (uv.y < 0.0 || uv.y > 1.0) shadow = 1.0;
                return _MainColor * max(0.2, dot(normalize(-_LightDir), normalize(IN.normalWS))) * (1.0 - shadow);
            }
            
            ENDHLSL
        }
    }
}