Shader "Custom/HorrorAtmosphereWithTrigger" {
    Properties {
        _MainTex ("Base Texture", 2D) = "white" {}
        
        // 新增：效果触发概率参数（0=几乎不触发，1=始终触发）
        _EffectTriggerChance ("Effect Trigger Chance (0-1)", Range(0,1)) = 0.7 
        
        // 核心恐怖效果参数
        _HorrorIntensity ("Horror Effect Strength", Range(0,1)) = 0.8 
        _DarkTint ("Dark Tint Color", Color) = (0.1, 0.05, 0.1, 0.7) 
        _VignetteIntensity ("Edge Darkening", Range(0,1)) = 0.6 
        
        // 故障干扰参数
        _GlitchIntensity ("Glitch Strength", Range(0,1)) = 0.4 
        _SliceCount ("Horizontal Slices", Range(3,100)) = 8 
        _SliceOffset ("Max Slice Shift", Range(0,0.3)) = 0.15 
        _ColorShift ("RGB Split", Range(0,0.05)) = 0.03 
        
        // 动态干扰参数
        _FlickerSpeed ("Flicker Speed", Range(1,30)) = 15 
        _FlickerIntensity ("Flicker Strength", Range(0,0.6)) = 0.3 
        _NoiseIntensity ("Film Noise", Range(0,0.3)) = 0.15 
        _Distortion ("Edge Warp", Range(0,0.08)) = 0.04 
        
        // 随机颜色错误参数
        _ColorErrorFreq ("Color Error Frequency", Range(0, 0.2)) = 0.05 
        _ColorErrorIntensity ("Color Error Strength", Range(0,1)) = 0.8 
        _ErrorColor1 ("Error Color 1 (Bloody Red)", Color) = (0.8, 0.05, 0.1, 1) 
        _ErrorColor2 ("Error Color 2 (Sickly Green)", Color) = (0.1, 0.7, 0.2, 1) 
        _ErrorBlockSize ("Error Block Size", Range(5, 50)) = 20 
    }

    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            // 新增触发参数
            float _EffectTriggerChance;
            
            // 核心参数
            float _HorrorIntensity;
            float4 _DarkTint;
            float _VignetteIntensity;
            
            // 故障参数
            float _GlitchIntensity;
            float _SliceCount;
            float _SliceOffset;
            float _ColorShift;
            
            // 动态参数
            float _FlickerSpeed;
            float _FlickerIntensity;
            float _NoiseIntensity;
            float _Distortion;
            
            // 颜色错误参数
            float _ColorErrorFreq;
            float _ColorErrorIntensity;
            float4 _ErrorColor1;
            float4 _ErrorColor2;
            float _ErrorBlockSize;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // 2D伪随机函数
            float rand(float2 co) {
                return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
            }

            fixed4 frag (v2f i) : SV_Target {
                float time = _Time.y;
                float2 uv = i.uv;
                float originalUVx = uv.x; // 保存原始UV用于"无效果"状态
                float originalUVy = uv.y;

                // ========== 核心：效果触发判断 ==========
                // 生成全局随机数（基于时间，确保同一帧内所有像素触发状态一致）
                float globalRand = rand(float2(time * 0.8, 456.78)); 
                // 触发逻辑：随机数 > 阈值时，效果生效（1=生效，0=关闭）
                float effectEnabled = step(_EffectTriggerChance, globalRand); 
                // 最终效果强度 = 恐怖强度 * 触发状态
                float h = _HorrorIntensity * effectEnabled;


                // 1. 边缘扭曲（仅在效果触发时生效）
                float warp = sin(uv.x * 15 + time * 3) * _Distortion * h;
                uv.y += warp * (1 - uv.x) * uv.x * 2;
                uv.x += sin(uv.y * 10 + time * 2) * _Distortion * 0.5 * h;

                // 2. 水平切块随机错位
                float slice = floor(uv.y * _SliceCount);
                float sliceRand = rand(float2(slice, time * 0.7));
                float shift = (sliceRand - 0.5) * 2 * _SliceOffset * _GlitchIntensity * h;
                uv.x += shift * (rand(float2(slice, time * 0.3)) > 0.3 ? 1 : 0);

                // 3. RGB通道分离
                float colorRand = rand(float2(floor(uv.y * 5), time * 0.5));
                float chromatic = colorRand * _ColorShift * _GlitchIntensity * h;
                // 若效果未触发，使用原始UV采样（无分离）
                fixed r = tex2D(_MainTex, effectEnabled ? (uv + float2(chromatic, 0)) : float2(originalUVx, originalUVy)).r;
                fixed g = tex2D(_MainTex, effectEnabled ? (uv + float2(-chromatic*0.5, 0)) : float2(originalUVx, originalUVy)).g;
                fixed b = tex2D(_MainTex, effectEnabled ? (uv - float2(chromatic, 0)) : float2(originalUVx, originalUVy)).b;
                fixed4 col = fixed4(r, g, b, 1);

                // 4. 胶片噪点
                float noise = rand(uv * 1000 + time * 2) * 2 - 1;
                float luminance = Luminance(col.rgb);
                col.rgb += noise * _NoiseIntensity * h * (1 - luminance * 0.8);

                // 5. 随机闪烁
                float flicker = rand(float2(time * _FlickerSpeed, 0.1)) * _FlickerIntensity;
                flicker = 1 - flicker * h * (rand(float2(time * 2, 0.2)) > 0.7 ? 1 : 0);
                col.rgb *= flicker;

                // 6. 暗色调偏移
                col.rgb = lerp(col.rgb, _DarkTint.rgb, _DarkTint.a * h * (1 - luminance * 0.5));

                // 7. 随机颜色错误
                float2 blockUV = floor(uv * _ErrorBlockSize) + time * 0.3;
                float blockRand = rand(blockUV);
                float isError = step(1 - _ColorErrorFreq, blockRand) * h;
                float colorChoice = rand(blockUV + 123.45);
                float3 errorColor = lerp(_ErrorColor1.rgb, _ErrorColor2.rgb, step(0.5, colorChoice));
                col.rgb = lerp(col.rgb, errorColor, isError * _ColorErrorIntensity);

                // 8. 暗角效果
                float2 centerDist = (uv - 0.5) * 2;
                float vignette = 1 - dot(centerDist, centerDist) * 0.8;
                vignette = smoothstep(0, 1, vignette);
                col.rgb *= lerp(1, vignette, _VignetteIntensity * h);

                return saturate(col);
            }
            ENDCG
        }
    }
}