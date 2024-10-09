Shader "Custom/VirtualTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = mul(unity_MatrixVP, mul(unity_ObjectToWorld, float4(IN.positionOS, 1.0)));
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float2 page = floor(IN.uv * _VTFeedbackParam.x);
	            float2 uv = IN.uv * _VTFeedbackParam.y;
	            float2 dx = ddx(uv);
	            float2 dy = ddy(uv);
	            float mip = clamp(0.5 * log2(max(dot(dx, dx), dot(dy, dy))), _VTFeedbackParam.w, _VTFeedbackParam.z);
                mip = floor(mip);
                page = floor(page / exp2(mip));
	            return float4(page / _VTFeedbackParam.xx, mip / _VTFeedbackParam.z, 1);
            }
            
            ENDHLSL
        }
        
        Pass
        {
            Tags
            {
                "LightMode" = "SRPDefaultUnlit"
            }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}