Shader "Custom/PearlInterference"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _LayerThickness ("Layer Thickness", Range(100, 1000)) = 500
        _LayerCount ("Layer Count", Range(1, 10)) = 5
        _RefractiveIndex ("Refractive Index", Range(1, 2)) = 1.5
        _InterferenceStrength ("Interference Strength", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float _Glossiness;
            float _Metallic;
            float _LayerThickness;
            int _LayerCount;
            float _RefractiveIndex;
            float _InterferenceStrength;
        CBUFFER_END
        
        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS : TEXCOORD0;
            float3 normalWS : TEXCOORD1;
            float3 tangentWS : TEXCOORD2;
            float3 bitangentWS : TEXCOORD3;
        };
        
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.tangentWS = TransformObjectToWorldDir(input.tangentOS.xyz);
                output.bitangentWS = cross(output.normalWS, output.tangentWS) * input.tangentOS.w;
                return output;
            }

            float3 PearlInterference(float cosTheta, float thickness, float refractiveIndex, int layerCount)
            {
                float3 wavelengths = float3(650, 510, 475); // RGB wavelengths in nanometers
                float3 phase = 4 * PI * refractiveIndex * thickness * layerCount / wavelengths;
                float3 interference = sin(phase / 2);
                interference *= interference; // Square for intensity
                return interference;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                float3 tangentWS = normalize(input.tangentWS);
                float3 bitangentWS = normalize(input.bitangentWS);
                float3 viewDirWS = normalize(GetWorldSpaceViewDir(input.positionWS));
                
                // Calculate view angle in tangent space
                float3 viewDirTS = float3(
                    dot(viewDirWS, tangentWS),
                    dot(viewDirWS, bitangentWS),
                    dot(viewDirWS, normalWS)
                );
                float viewAngle = atan2(length(viewDirTS.xy), viewDirTS.z);
                
                // Calculate interference based on view angle
                float cosTheta = cos(viewAngle);
                float3 interference = PearlInterference(cosTheta, _LayerThickness, _RefractiveIndex, _LayerCount);
                
                // Basic lighting
                Light mainLight = GetMainLight();
                float3 lightDirWS = mainLight.direction;
                float3 halfVectorWS = normalize(viewDirWS + lightDirWS);
                
                float NdotL = saturate(dot(normalWS, lightDirWS));
                float NdotH = saturate(dot(normalWS, halfVectorWS));
                float NdotV = saturate(dot(normalWS, viewDirWS));
                
                // Fresnel effect
                float3 F0 = lerp(0.04, _BaseColor.rgb, _Metallic);
                float3 F = F0 + (1 - F0) * pow(1 - NdotV, 5);
                
                float3 diffuse = _BaseColor.rgb * (1 - _Metallic) * NdotL;
                float3 specular = F * pow(NdotH, _Glossiness * 100) * NdotL;
                
                // Combine base color, lighting, and interference
                float3 finalColor = lerp(diffuse + specular, interference, _InterferenceStrength) * mainLight.color;
                
                return float4(finalColor, 1);
            }
            ENDHLSL
        }
    }
}