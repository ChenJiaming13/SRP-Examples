Shader "Custom/InverseColor"
{
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
            };

            float4x4 unity_ObjectToWorld;
            float4x4 unity_MatrixVP;
            sampler2D _MainTex;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = mul(unity_MatrixVP, mul(unity_ObjectToWorld, IN.positionOS));
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float4 color = tex2D(_MainTex, IN.uv);
                return 1 - float4(color);
            }
            
            ENDHLSL
        }
    }
}
