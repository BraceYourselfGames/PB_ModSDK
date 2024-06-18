Shader "Custom/Vertex Color Debug" { // defines the name of the shader 
   Properties {
   }
   SubShader {
      Pass {	
         Tags { "LightMode" = "ForwardBase" } 
            // make sure that all uniforms are correctly set
 
         CGPROGRAM
 
         #pragma vertex vert  
         #pragma fragment frag 
 
         #include "UnityCG.cginc"
 
         struct vertexInput {
            float4 vertex : POSITION;
            float4 color : COLOR;
         };
         struct vertexOutput {
            float4 pos : SV_POSITION;
            float4 col : COLOR;
         };
 
         vertexOutput vert(vertexInput input) 
         {
            vertexOutput output;
 
            float4x4 modelMatrix = unity_ObjectToWorld;
            float4x4 modelMatrixInverse = unity_WorldToObject;

 
            output.col = input.color;
            output.pos = UnityObjectToClipPos(input.vertex);
            return output;
         }
 
         float4 frag(vertexOutput input) : COLOR
         {
            float4 final = float4(input.col.rgb, 1);//float4(1,1,1,1);
            return final;
         }
 
         ENDCG
      }
   }
   Fallback "Diffuse"
}