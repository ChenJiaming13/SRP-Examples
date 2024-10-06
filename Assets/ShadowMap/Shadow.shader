Shader "Custom/Shadow"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (1, 1, 1, 1)
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
                float3 normal: NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS: SV_POSITION;
                float3 normal: TEXCOORD0;
                float4 positionLS: TEXCOORD1;
            };

            float4x4 unity_ObjectToWorld;
            float4x4 unity_MatrixVP;
            float4 _MainColor;
            float3 _LightDir;
            float4x4 _LightMatrix;
            sampler2D _ShadowMap;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                const float4 positionWS = mul(unity_ObjectToWorld, IN.positionOS);
                OUT.positionHCS = mul(unity_MatrixVP, positionWS);
                OUT.normal = IN.normal;
                OUT.positionLS = mul(_LightMatrix, positionWS);
                return OUT;
            }

            float4 frag(Varyings IN): SV_Target
            {
                float3 ndc = IN.positionLS.xyz / IN.positionLS.w;
                ndc = ndc * 0.5 + 0.5;
                float2 uv = ndc.xy;
                uv.y = 1.0 - uv.y;
                float closestDepth = tex2D(_ShadowMap, uv).r;
                float currDepth = ndc.z;
                closestDepth = 1.0 - closestDepth;
                currDepth = 1.0 - currDepth;
                float shadow = currDepth > closestDepth ? 1.0 : 0.0;
                if (ndc.x < 0.0 || ndc.x > 1.0) shadow = 1.0;
                if (ndc.y < 0.0 || ndc.y > 1.0) shadow = 1.0;
                return _MainColor * max(0.2, dot(normalize(-_LightDir), normalize(IN.normal))) * (1.0 - shadow);
            }
            
            ENDHLSL
        }
    }
}