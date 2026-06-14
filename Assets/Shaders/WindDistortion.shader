Shader "Custom/WindDistortion"
{
    Properties
    {
        _DistortionStrength ("Distortion Strength", Range(0, 0.1)) = 0.02
        _Speed ("Speed", Range(0, 10)) = 3.0
        _NoiseScale ("Noise Scale", Range(0.1, 10)) = 2.0
        _Direction ("Direction", Vector) = (0, 1, 0, 0)
        _FadeStart ("Fade Start", Range(0, 1)) = 0.3
        _FadeEnd ("Fade End", Range(0, 1)) = 0.9
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent+100"
            "IgnoreProjector" = "True"
        }
        
        GrabPass { "_GrabTexture" }
        
        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 grabPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float4 color : COLOR;
            };
            
            sampler2D _GrabTexture;
            float4 _GrabTexture_TexelSize;
            
            float _DistortionStrength;
            float _Speed;
            float _NoiseScale;
            float4 _Direction;
            float _FadeStart;
            float _FadeEnd;
            
            // Простой шум
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }
            
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }
            
            // Фрактальный шум для более интересного эффекта
            float fbm(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                
                for (int i = 0; i < 3; i++)
                {
                    value += amplitude * noise(p);
                    p *= 2.0;
                    amplitude *= 0.5;
                }
                
                return value;
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // Движение шума по направлению ветра
                float2 uvOffset = _Direction.xy * _Time.y * _Speed;
                
                // Генерируем искажение на основе шума
                float2 noiseUV = i.uv * _NoiseScale + uvOffset;
                float n1 = fbm(noiseUV);
                float n2 = fbm(noiseUV + float2(5.2, 1.3));
                
                // Смещение UV для grab texture
                float2 distortion = float2(n1 - 0.5, n2 - 0.5) * 2.0;
                distortion *= _DistortionStrength;
                
                // Затухание по вертикали (от источника к краю)
                float fade = smoothstep(_FadeStart, _FadeEnd, i.uv.y);
                fade = 1.0 - fade; // Инвертируем: сильнее у источника
                
                // Также затухание по краям по горизонтали
                float edgeFade = smoothstep(0.0, 0.2, i.uv.x) * smoothstep(1.0, 0.8, i.uv.x);
                
                float totalFade = fade * edgeFade * i.color.a;
                distortion *= totalFade;
                
                // Применяем искажение
                float2 grabUV = i.grabPos.xy / i.grabPos.w;
                grabUV += distortion;
                
                fixed4 col = tex2D(_GrabTexture, grabUV);
                
                // Можно добавить лёгкий оттенок
                col.rgb = lerp(col.rgb, col.rgb * float3(0.95, 0.97, 1.0), totalFade * 0.3);
                
                return col;
            }
            ENDCG
        }
    }
    
    FallBack "Transparent/Diffuse"
}
