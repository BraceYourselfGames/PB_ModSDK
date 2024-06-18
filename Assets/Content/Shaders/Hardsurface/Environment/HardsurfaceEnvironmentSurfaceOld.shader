// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Shader created with Shader Forge v1.27 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.27;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:3,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:True,hqlp:False,rprd:True,enco:False,rmgx:True,rpth:1,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:2865,x:33570,y:32789,varname:node_2865,prsc:2|diff-6350-OUT,spec-1080-R,gloss-5163-OUT,emission-8659-OUT,difocc-1080-A,spcocc-1080-A;n:type:ShaderForge.SFN_Tex2d,id:7736,x:30703,y:32509,ptovrint:True,ptlb:AH,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:23e060dbadfcfe3458b0fba2388d85f0,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:1080,x:31278,y:33512,ptovrint:False,ptlb:MSEO,ptin:_MSEO,varname:node_1080,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:80cf3326e49cbec418b76f2d71b84cf3,ntxv:0,isnm:False;n:type:ShaderForge.SFN_RgbToHsv,id:360,x:31238,y:32351,varname:node_360,prsc:2|IN-7736-RGB;n:type:ShaderForge.SFN_Slider,id:3007,x:31081,y:32075,ptovrint:False,ptlb:HueOffsetPrimary,ptin:_HueOffsetPrimary,varname:node_3007,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-0.5,cur:0,max:0.5;n:type:ShaderForge.SFN_Add,id:8819,x:31758,y:31914,varname:node_8819,prsc:2|A-3007-OUT,B-7913-R,C-360-HOUT;n:type:ShaderForge.SFN_Frac,id:4084,x:31951,y:31914,varname:node_4084,prsc:2|IN-8819-OUT;n:type:ShaderForge.SFN_HsvToRgb,id:5621,x:32577,y:32538,varname:node_5621,prsc:2|H-4084-OUT,S-4615-OUT,V-360-VOUT;n:type:ShaderForge.SFN_Slider,id:7287,x:31121,y:33254,ptovrint:False,ptlb:SmoothnessMin,ptin:_SmoothnessMin,varname:node_7287,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.2,max:1;n:type:ShaderForge.SFN_Slider,id:3937,x:31121,y:33355,ptovrint:False,ptlb:SmoothnessMax,ptin:_SmoothnessMax,varname:_SmoothnessMin_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.8,max:1;n:type:ShaderForge.SFN_Lerp,id:5163,x:33169,y:32982,varname:node_5163,prsc:2|A-7287-OUT,B-3937-OUT,T-1080-G;n:type:ShaderForge.SFN_Lerp,id:9189,x:32845,y:32705,varname:node_9189,prsc:2|A-7736-RGB,B-5621-OUT,T-4328-OUT;n:type:ShaderForge.SFN_Multiply,id:8659,x:33169,y:33201,varname:node_8659,prsc:2|A-1080-B,B-4681-RGB,C-2106-OUT,D-9951-OUT;n:type:ShaderForge.SFN_Slider,id:2106,x:31121,y:33892,ptovrint:False,ptlb:EmissionIntensity,ptin:_EmissionIntensity,varname:_SmoothnessMax_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:16;n:type:ShaderForge.SFN_Color,id:4681,x:31278,y:33698,ptovrint:False,ptlb:EmissionColor,ptin:_EmissionColor,varname:node_4681,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:0,c3:0,c4:1;n:type:ShaderForge.SFN_Slider,id:4926,x:31081,y:32181,ptovrint:False,ptlb:SaturationOffsetPrimary,ptin:_SaturationOffsetPrimary,varname:node_4926,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Lerp,id:4820,x:31951,y:32238,varname:node_4820,prsc:2|A-9168-OUT,B-360-SOUT,T-4926-OUT;n:type:ShaderForge.SFN_Vector1,id:9168,x:31754,y:32238,varname:node_9168,prsc:2,v1:0;n:type:ShaderForge.SFN_VertexColor,id:7913,x:31232,y:31923,varname:node_7913,prsc:2;n:type:ShaderForge.SFN_Multiply,id:4615,x:31951,y:32080,varname:node_4615,prsc:2|A-7913-G,B-4820-OUT;n:type:ShaderForge.SFN_Slider,id:4494,x:31121,y:33139,ptovrint:False,ptlb:ColorVertexInfluence,ptin:_ColorVertexInfluence,varname:node_4494,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Slider,id:444,x:31121,y:33994,ptovrint:False,ptlb:EmissionVertexInfluence,ptin:_EmissionVertexInfluence,varname:_EmissionIntensity_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Lerp,id:9951,x:31963,y:33926,varname:node_9951,prsc:2|A-483-OUT,B-9527-U,T-444-OUT;n:type:ShaderForge.SFN_Vector1,id:483,x:31963,y:33861,varname:node_483,prsc:2,v1:1;n:type:ShaderForge.SFN_TexCoord,id:9527,x:31278,y:34083,varname:node_9527,prsc:2,uv:1;n:type:ShaderForge.SFN_Slider,id:2228,x:31075,y:31656,ptovrint:False,ptlb:HueOffsetSecondary,ptin:_HueOffsetSecondary,varname:_HueOffset_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-0.5,cur:0,max:0.5;n:type:ShaderForge.SFN_Slider,id:5753,x:31075,y:31757,ptovrint:False,ptlb:SaturationOffsetSecondary,ptin:_SaturationOffsetSecondary,varname:_SaturationOffsetPrimary_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Add,id:7318,x:31752,y:31235,varname:node_7318,prsc:2|A-2228-OUT,B-7913-B;n:type:ShaderForge.SFN_Frac,id:2758,x:31951,y:31235,varname:node_2758,prsc:2|IN-7318-OUT;n:type:ShaderForge.SFN_Lerp,id:8746,x:31951,y:31550,varname:node_8746,prsc:2|A-8710-OUT,B-360-SOUT,T-5753-OUT;n:type:ShaderForge.SFN_Multiply,id:2460,x:31951,y:31392,varname:node_2460,prsc:2|A-7913-A,B-8746-OUT;n:type:ShaderForge.SFN_Vector1,id:8710,x:31748,y:31550,varname:node_8710,prsc:2,v1:0;n:type:ShaderForge.SFN_HsvToRgb,id:1168,x:32577,y:32389,varname:node_1168,prsc:2|H-2758-OUT,S-2460-OUT,V-360-VOUT;n:type:ShaderForge.SFN_Multiply,id:4328,x:32428,y:32751,varname:node_4328,prsc:2|A-7440-OUT,B-9804-OUT;n:type:ShaderForge.SFN_Vector1,id:9804,x:32428,y:32887,varname:node_9804,prsc:2,v1:2;n:type:ShaderForge.SFN_Lerp,id:5830,x:31737,y:32754,varname:node_5830,prsc:2|A-3306-OUT,B-7736-A,T-4494-OUT;n:type:ShaderForge.SFN_Vector1,id:3306,x:31737,y:32686,varname:node_3306,prsc:2,v1:0.5;n:type:ShaderForge.SFN_Vector1,id:5860,x:32024,y:32685,varname:node_5860,prsc:2,v1:-0.5;n:type:ShaderForge.SFN_Multiply,id:1319,x:32024,y:32961,varname:node_1319,prsc:2|A-9804-OUT,B-5830-OUT;n:type:ShaderForge.SFN_Add,id:6663,x:32024,y:32751,varname:node_6663,prsc:2|A-5830-OUT,B-5860-OUT;n:type:ShaderForge.SFN_Clamp01,id:1670,x:32225,y:32961,varname:node_1670,prsc:2|IN-1319-OUT;n:type:ShaderForge.SFN_Lerp,id:6350,x:33169,y:32764,varname:node_6350,prsc:2|A-9189-OUT,B-1168-OUT,T-6387-OUT;n:type:ShaderForge.SFN_OneMinus,id:6387,x:32428,y:32961,varname:node_6387,prsc:2|IN-1670-OUT;n:type:ShaderForge.SFN_Clamp01,id:7440,x:32225,y:32751,varname:node_7440,prsc:2|IN-6663-OUT;proporder:7736-1080-7287-3937-3007-4926-2228-5753-2106-4681-4494-444;pass:END;sub:END;*/

Shader "Hardsurface/Environment/Surface (Old)" {
    Properties {
        _MainTex ("AH", 2D) = "white" {}
        _MSEO ("MSEO", 2D) = "white" {}
        _SmoothnessMin ("SmoothnessMin", Range(0, 1)) = 0.2
        _SmoothnessMax ("SmoothnessMax", Range(0, 1)) = 0.8
        _HueOffsetPrimary ("HueOffsetPrimary", Range(-0.5, 0.5)) = 0
        _SaturationOffsetPrimary ("SaturationOffsetPrimary", Range(0, 1)) = 1
        _HueOffsetSecondary ("HueOffsetSecondary", Range(-0.5, 0.5)) = 0
        _SaturationOffsetSecondary ("SaturationOffsetSecondary", Range(0, 1)) = 1
        _EmissionIntensity ("EmissionIntensity", Range(0, 16)) = 0
        _EmissionColor ("EmissionColor", Color) = (0,0,0,1)
        _ColorVertexInfluence ("ColorVertexInfluence", Range(0, 1)) = 1
        _EmissionVertexInfluence ("EmissionVertexInfluence", Range(0, 1)) = 0
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "DEFERRED"
            Tags {
                "LightMode"="Deferred"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile ___ UNITY_HDR_ON
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _MSEO; uniform float4 _MSEO_ST;
            uniform float _HueOffsetPrimary;
            uniform float _SmoothnessMin;
            uniform float _SmoothnessMax;
            uniform float _EmissionIntensity;
            uniform float4 _EmissionColor;
            uniform float _SaturationOffsetPrimary;
            uniform float _ColorVertexInfluence;
            uniform float _EmissionVertexInfluence;
            uniform float _HueOffsetSecondary;
            uniform float _SaturationOffsetSecondary;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                float4 vertexColor : COLOR;
                #if defined(LIGHTMAP_ON) || defined(UNITY_SHOULD_SAMPLE_SH)
                    float4 ambientOrLightmapUV : TEXCOORD7;
                #endif
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.vertexColor = v.vertexColor;
                #ifdef LIGHTMAP_ON
                    o.ambientOrLightmapUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                    o.ambientOrLightmapUV.zw = 0;
                #elif UNITY_SHOULD_SAMPLE_SH
                #endif
                #ifdef DYNAMICLIGHTMAP_ON
                    o.ambientOrLightmapUV.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                #endif
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex );
                return o;
            }
            void frag(
                VertexOutput i,
                out half4 outDiffuse : SV_Target0,
                out half4 outSpecSmoothness : SV_Target1,
                out half4 outNormal : SV_Target2,
                out half4 outEmission : SV_Target3 )
            {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
////// Lighting:
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float4 _MSEO_var = tex2D(_MSEO,TRANSFORM_TEX(i.uv0, _MSEO));
                float gloss = lerp(_SmoothnessMin,_SmoothnessMax,_MSEO_var.g);
/////// GI Data:
                UnityLight light; // Dummy light
                light.color = 0;
                light.dir = half3(0,1,0);
                light.ndotl = max(0,dot(normalDirection,light.dir));
                UnityGIInput d;
                d.light = light;
                d.worldPos = i.posWorld.xyz;
                d.worldViewDir = viewDirection;
                d.atten = 1;
                #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                    d.ambient = 0;
                    d.lightmapUV = i.ambientOrLightmapUV;
                #else
                    d.ambient = i.ambientOrLightmapUV;
                #endif
                d.boxMax[0] = unity_SpecCube0_BoxMax;
                d.boxMin[0] = unity_SpecCube0_BoxMin;
                d.probePosition[0] = unity_SpecCube0_ProbePosition;
                d.probeHDR[0] = unity_SpecCube0_HDR;
                d.boxMax[1] = unity_SpecCube1_BoxMax;
                d.boxMin[1] = unity_SpecCube1_BoxMin;
                d.probePosition[1] = unity_SpecCube1_ProbePosition;
                d.probeHDR[1] = unity_SpecCube1_HDR;
                Unity_GlossyEnvironmentData ugls_en_data;
                ugls_en_data.roughness = 1.0 - gloss;
                ugls_en_data.reflUVW = viewReflectDirection;
                UnityGI gi = UnityGlobalIllumination(d, 1, normalDirection, ugls_en_data );
////// Specular:
                float3 specularColor = _MSEO_var.r;
                float specularMonochrome;
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float4 node_360_k = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 node_360_p = lerp(float4(float4(_MainTex_var.rgb,0.0).zy, node_360_k.wz), float4(float4(_MainTex_var.rgb,0.0).yz, node_360_k.xy), step(float4(_MainTex_var.rgb,0.0).z, float4(_MainTex_var.rgb,0.0).y));
                float4 node_360_q = lerp(float4(node_360_p.xyw, float4(_MainTex_var.rgb,0.0).x), float4(float4(_MainTex_var.rgb,0.0).x, node_360_p.yzx), step(node_360_p.x, float4(_MainTex_var.rgb,0.0).x));
                float node_360_d = node_360_q.x - min(node_360_q.w, node_360_q.y);
                float node_360_e = 1.0e-10;
                float3 node_360 = float3(abs(node_360_q.z + (node_360_q.w - node_360_q.y) / (6.0 * node_360_d + node_360_e)), node_360_d / (node_360_q.x + node_360_e), node_360_q.x);;
                float node_5830 = lerp(0.5,_MainTex_var.a,_ColorVertexInfluence);
                float node_9804 = 2.0;
                float3 diffuseColor = lerp(lerp(_MainTex_var.rgb,(lerp(float3(1,1,1),saturate(3.0*abs(1.0-2.0*frac(frac((_HueOffsetPrimary+i.vertexColor.r+node_360.r))+float3(0.0,-1.0/3.0,1.0/3.0)))-1),(i.vertexColor.g*lerp(0.0,node_360.g,_SaturationOffsetPrimary)))*node_360.b),(saturate((node_5830+(-0.5)))*node_9804)),(lerp(float3(1,1,1),saturate(3.0*abs(1.0-2.0*frac(frac((_HueOffsetSecondary+i.vertexColor.b))+float3(0.0,-1.0/3.0,1.0/3.0)))-1),(i.vertexColor.a*lerp(0.0,node_360.g,_SaturationOffsetSecondary)))*node_360.b),(1.0 - saturate((node_9804*node_5830)))); // Need this for specular when using metallic
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, specularColor, specularColor, specularMonochrome );
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = max(0.0,dot( normalDirection, viewDirection ));
                half grazingTerm = saturate( gloss + specularMonochrome );
                float3 indirectSpecular = (gi.indirect.specular);
                indirectSpecular *= FresnelLerp (specularColor, grazingTerm, NdotV);
/////// Diffuse:
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += gi.indirect.diffuse;
                indirectDiffuse *= _MSEO_var.a; // Diffuse AO
////// Emissive:
                float3 emissive = (_MSEO_var.b*_EmissionColor.rgb*_EmissionIntensity*lerp(1.0,i.uv1.r,_EmissionVertexInfluence));
/// Final Color:
                outDiffuse = half4( diffuseColor, _MSEO_var.a );
                outSpecSmoothness = half4( specularColor, gloss );
                outNormal = half4( normalDirection * 0.5 + 0.5, 1 );
                outEmission = half4( (_MSEO_var.b*_EmissionColor.rgb*_EmissionIntensity*lerp(1.0,i.uv1.r,_EmissionVertexInfluence)), 1 );
                outEmission.rgb += indirectSpecular * _MSEO_var.a;
                outEmission.rgb += indirectDiffuse * diffuseColor;
                #ifndef UNITY_HDR_ON
                    outEmission.rgb = exp2(-outEmission.rgb);
                #endif
            }
            ENDCG
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _MSEO; uniform float4 _MSEO_ST;
            uniform float _HueOffsetPrimary;
            uniform float _SmoothnessMin;
            uniform float _SmoothnessMax;
            uniform float _EmissionIntensity;
            uniform float4 _EmissionColor;
            uniform float _SaturationOffsetPrimary;
            uniform float _ColorVertexInfluence;
            uniform float _EmissionVertexInfluence;
            uniform float _HueOffsetSecondary;
            uniform float _SaturationOffsetSecondary;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                float4 vertexColor : COLOR;
                LIGHTING_COORDS(7,8)
                UNITY_FOG_COORDS(9)
                #if defined(LIGHTMAP_ON) || defined(UNITY_SHOULD_SAMPLE_SH)
                    float4 ambientOrLightmapUV : TEXCOORD10;
                #endif
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.vertexColor = v.vertexColor;
                #ifdef LIGHTMAP_ON
                    o.ambientOrLightmapUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                    o.ambientOrLightmapUV.zw = 0;
                #elif UNITY_SHOULD_SAMPLE_SH
                #endif
                #ifdef DYNAMICLIGHTMAP_ON
                    o.ambientOrLightmapUV.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                #endif
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float4 _MSEO_var = tex2D(_MSEO,TRANSFORM_TEX(i.uv0, _MSEO));
                float gloss = lerp(_SmoothnessMin,_SmoothnessMax,_MSEO_var.g);
                float specPow = exp2( gloss * 10.0+1.0);
/////// GI Data:
                UnityLight light;
                #ifdef LIGHTMAP_OFF
                    light.color = lightColor;
                    light.dir = lightDirection;
                    light.ndotl = LambertTerm (normalDirection, light.dir);
                #else
                    light.color = half3(0.f, 0.f, 0.f);
                    light.ndotl = 0.0f;
                    light.dir = half3(0.f, 0.f, 0.f);
                #endif
                UnityGIInput d;
                d.light = light;
                d.worldPos = i.posWorld.xyz;
                d.worldViewDir = viewDirection;
                d.atten = attenuation;
                #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                    d.ambient = 0;
                    d.lightmapUV = i.ambientOrLightmapUV;
                #else
                    d.ambient = i.ambientOrLightmapUV;
                #endif
                d.boxMax[0] = unity_SpecCube0_BoxMax;
                d.boxMin[0] = unity_SpecCube0_BoxMin;
                d.probePosition[0] = unity_SpecCube0_ProbePosition;
                d.probeHDR[0] = unity_SpecCube0_HDR;
                d.boxMax[1] = unity_SpecCube1_BoxMax;
                d.boxMin[1] = unity_SpecCube1_BoxMin;
                d.probePosition[1] = unity_SpecCube1_ProbePosition;
                d.probeHDR[1] = unity_SpecCube1_HDR;
                Unity_GlossyEnvironmentData ugls_en_data;
                ugls_en_data.roughness = 1.0 - gloss;
                ugls_en_data.reflUVW = viewReflectDirection;
                UnityGI gi = UnityGlobalIllumination(d, 1, normalDirection, ugls_en_data );
                lightDirection = gi.light.dir;
                lightColor = gi.light.color;
////// Specular:
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float3 specularAO = _MSEO_var.a;
                float LdotH = max(0.0,dot(lightDirection, halfDirection));
                float3 specularColor = _MSEO_var.r;
                float specularMonochrome;
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float4 node_360_k = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 node_360_p = lerp(float4(float4(_MainTex_var.rgb,0.0).zy, node_360_k.wz), float4(float4(_MainTex_var.rgb,0.0).yz, node_360_k.xy), step(float4(_MainTex_var.rgb,0.0).z, float4(_MainTex_var.rgb,0.0).y));
                float4 node_360_q = lerp(float4(node_360_p.xyw, float4(_MainTex_var.rgb,0.0).x), float4(float4(_MainTex_var.rgb,0.0).x, node_360_p.yzx), step(node_360_p.x, float4(_MainTex_var.rgb,0.0).x));
                float node_360_d = node_360_q.x - min(node_360_q.w, node_360_q.y);
                float node_360_e = 1.0e-10;
                float3 node_360 = float3(abs(node_360_q.z + (node_360_q.w - node_360_q.y) / (6.0 * node_360_d + node_360_e)), node_360_d / (node_360_q.x + node_360_e), node_360_q.x);;
                float node_5830 = lerp(0.5,_MainTex_var.a,_ColorVertexInfluence);
                float node_9804 = 2.0;
                float3 diffuseColor = lerp(lerp(_MainTex_var.rgb,(lerp(float3(1,1,1),saturate(3.0*abs(1.0-2.0*frac(frac((_HueOffsetPrimary+i.vertexColor.r+node_360.r))+float3(0.0,-1.0/3.0,1.0/3.0)))-1),(i.vertexColor.g*lerp(0.0,node_360.g,_SaturationOffsetPrimary)))*node_360.b),(saturate((node_5830+(-0.5)))*node_9804)),(lerp(float3(1,1,1),saturate(3.0*abs(1.0-2.0*frac(frac((_HueOffsetSecondary+i.vertexColor.b))+float3(0.0,-1.0/3.0,1.0/3.0)))-1),(i.vertexColor.a*lerp(0.0,node_360.g,_SaturationOffsetSecondary)))*node_360.b),(1.0 - saturate((node_9804*node_5830)))); // Need this for specular when using metallic
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, specularColor, specularColor, specularMonochrome );
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = max(0.0,dot( normalDirection, viewDirection ));
                float NdotH = max(0.0,dot( normalDirection, halfDirection ));
                float VdotH = max(0.0,dot( viewDirection, halfDirection ));
                float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, 1.0-gloss );
                float normTerm = max(0.0, GGXTerm(NdotH, 1.0-gloss));
                float specularPBL = (NdotL*visTerm*normTerm) * (UNITY_PI / 4);
                if (IsGammaSpace())
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                specularPBL = max(0, specularPBL * NdotL);
                float3 directSpecular = 1*specularPBL*lightColor*FresnelTerm(specularColor, LdotH);
                half grazingTerm = saturate( gloss + specularMonochrome );
                float3 indirectSpecular = (gi.indirect.specular) * specularAO;
                indirectSpecular *= FresnelLerp (specularColor, grazingTerm, NdotV);
                float3 specular = (directSpecular + indirectSpecular);
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += gi.indirect.diffuse;
                indirectDiffuse *= _MSEO_var.a; // Diffuse AO
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
////// Emissive:
                float3 emissive = (_MSEO_var.b*_EmissionColor.rgb*_EmissionIntensity*lerp(1.0,i.uv1.r,_EmissionVertexInfluence));
/// Final Color:
                float3 finalColor = diffuse + specular + emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _MSEO; uniform float4 _MSEO_ST;
            uniform float _HueOffsetPrimary;
            uniform float _SmoothnessMin;
            uniform float _SmoothnessMax;
            uniform float _EmissionIntensity;
            uniform float4 _EmissionColor;
            uniform float _SaturationOffsetPrimary;
            uniform float _ColorVertexInfluence;
            uniform float _EmissionVertexInfluence;
            uniform float _HueOffsetSecondary;
            uniform float _SaturationOffsetSecondary;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                float4 vertexColor : COLOR;
                LIGHTING_COORDS(7,8)
                UNITY_FOG_COORDS(9)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.vertexColor = v.vertexColor;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float4 _MSEO_var = tex2D(_MSEO,TRANSFORM_TEX(i.uv0, _MSEO));
                float gloss = lerp(_SmoothnessMin,_SmoothnessMax,_MSEO_var.g);
                float specPow = exp2( gloss * 10.0+1.0);
////// Specular:
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float LdotH = max(0.0,dot(lightDirection, halfDirection));
                float3 specularColor = _MSEO_var.r;
                float specularMonochrome;
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float4 node_360_k = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 node_360_p = lerp(float4(float4(_MainTex_var.rgb,0.0).zy, node_360_k.wz), float4(float4(_MainTex_var.rgb,0.0).yz, node_360_k.xy), step(float4(_MainTex_var.rgb,0.0).z, float4(_MainTex_var.rgb,0.0).y));
                float4 node_360_q = lerp(float4(node_360_p.xyw, float4(_MainTex_var.rgb,0.0).x), float4(float4(_MainTex_var.rgb,0.0).x, node_360_p.yzx), step(node_360_p.x, float4(_MainTex_var.rgb,0.0).x));
                float node_360_d = node_360_q.x - min(node_360_q.w, node_360_q.y);
                float node_360_e = 1.0e-10;
                float3 node_360 = float3(abs(node_360_q.z + (node_360_q.w - node_360_q.y) / (6.0 * node_360_d + node_360_e)), node_360_d / (node_360_q.x + node_360_e), node_360_q.x);;
                float node_5830 = lerp(0.5,_MainTex_var.a,_ColorVertexInfluence);
                float node_9804 = 2.0;
                float3 diffuseColor = lerp(lerp(_MainTex_var.rgb,(lerp(float3(1,1,1),saturate(3.0*abs(1.0-2.0*frac(frac((_HueOffsetPrimary+i.vertexColor.r+node_360.r))+float3(0.0,-1.0/3.0,1.0/3.0)))-1),(i.vertexColor.g*lerp(0.0,node_360.g,_SaturationOffsetPrimary)))*node_360.b),(saturate((node_5830+(-0.5)))*node_9804)),(lerp(float3(1,1,1),saturate(3.0*abs(1.0-2.0*frac(frac((_HueOffsetSecondary+i.vertexColor.b))+float3(0.0,-1.0/3.0,1.0/3.0)))-1),(i.vertexColor.a*lerp(0.0,node_360.g,_SaturationOffsetSecondary)))*node_360.b),(1.0 - saturate((node_9804*node_5830)))); // Need this for specular when using metallic
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, specularColor, specularColor, specularMonochrome );
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = max(0.0,dot( normalDirection, viewDirection ));
                float NdotH = max(0.0,dot( normalDirection, halfDirection ));
                float VdotH = max(0.0,dot( viewDirection, halfDirection ));
                float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, 1.0-gloss );
                float normTerm = max(0.0, GGXTerm(NdotH, 1.0-gloss));
                float specularPBL = (NdotL*visTerm*normTerm) * (UNITY_PI / 4);
                if (IsGammaSpace())
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                specularPBL = max(0, specularPBL * NdotL);
                float3 directSpecular = attenColor*specularPBL*lightColor*FresnelTerm(specularColor, LdotH);
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * attenColor;
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                fixed4 finalRGBA = fixed4(finalColor * 1,0);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "Meta"
            Tags {
                "LightMode"="Meta"
            }
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_META 1
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #include "UnityMetaPass.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _MSEO; uniform float4 _MSEO_ST;
            uniform float _HueOffsetPrimary;
            uniform float _SmoothnessMin;
            uniform float _SmoothnessMax;
            uniform float _EmissionIntensity;
            uniform float4 _EmissionColor;
            uniform float _SaturationOffsetPrimary;
            uniform float _ColorVertexInfluence;
            uniform float _EmissionVertexInfluence;
            uniform float _HueOffsetSecondary;
            uniform float _SaturationOffsetSecondary;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.vertexColor = v.vertexColor;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST );
                return o;
            }
            float4 frag(VertexOutput i) : SV_Target {
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                UnityMetaInput o;
                UNITY_INITIALIZE_OUTPUT( UnityMetaInput, o );
                
                float4 _MSEO_var = tex2D(_MSEO,TRANSFORM_TEX(i.uv0, _MSEO));
                o.Emission = (_MSEO_var.b*_EmissionColor.rgb*_EmissionIntensity*lerp(1.0,i.uv1.r,_EmissionVertexInfluence));
                
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float4 node_360_k = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 node_360_p = lerp(float4(float4(_MainTex_var.rgb,0.0).zy, node_360_k.wz), float4(float4(_MainTex_var.rgb,0.0).yz, node_360_k.xy), step(float4(_MainTex_var.rgb,0.0).z, float4(_MainTex_var.rgb,0.0).y));
                float4 node_360_q = lerp(float4(node_360_p.xyw, float4(_MainTex_var.rgb,0.0).x), float4(float4(_MainTex_var.rgb,0.0).x, node_360_p.yzx), step(node_360_p.x, float4(_MainTex_var.rgb,0.0).x));
                float node_360_d = node_360_q.x - min(node_360_q.w, node_360_q.y);
                float node_360_e = 1.0e-10;
                float3 node_360 = float3(abs(node_360_q.z + (node_360_q.w - node_360_q.y) / (6.0 * node_360_d + node_360_e)), node_360_d / (node_360_q.x + node_360_e), node_360_q.x);;
                float node_5830 = lerp(0.5,_MainTex_var.a,_ColorVertexInfluence);
                float node_9804 = 2.0;
                float3 diffColor = lerp(lerp(_MainTex_var.rgb,(lerp(float3(1,1,1),saturate(3.0*abs(1.0-2.0*frac(frac((_HueOffsetPrimary+i.vertexColor.r+node_360.r))+float3(0.0,-1.0/3.0,1.0/3.0)))-1),(i.vertexColor.g*lerp(0.0,node_360.g,_SaturationOffsetPrimary)))*node_360.b),(saturate((node_5830+(-0.5)))*node_9804)),(lerp(float3(1,1,1),saturate(3.0*abs(1.0-2.0*frac(frac((_HueOffsetSecondary+i.vertexColor.b))+float3(0.0,-1.0/3.0,1.0/3.0)))-1),(i.vertexColor.a*lerp(0.0,node_360.g,_SaturationOffsetSecondary)))*node_360.b),(1.0 - saturate((node_9804*node_5830))));
                float specularMonochrome;
                float3 specColor;
                diffColor = DiffuseAndSpecularFromMetallic( diffColor, _MSEO_var.r, specColor, specularMonochrome );
                float roughness = 1.0 - lerp(_SmoothnessMin,_SmoothnessMax,_MSEO_var.g);
                o.Albedo = diffColor + specColor * roughness * roughness * 0.5;
                
                return UnityMetaFragment( o );
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    // CustomEditor "ShaderForgeMaterialInspector"
}
