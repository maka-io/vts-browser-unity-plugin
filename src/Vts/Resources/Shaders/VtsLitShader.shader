Shader "Vts/LitShader"
{
	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
			"LightMode" = "ForwardBase"
		}
		LOD 100

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
			#include "AutoLight.cginc"

			#include "VtsCommon.cginc"
			#pragma multi_compile __ VTS_ATMOSPHERE
			#include "VtsAtmosphere.cginc"

			struct vIn
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				VTS_VIN_UV
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 normal : NORMAL;
				VTS_V2F_COMMON
				VTS_V2F_CLIP
				SHADOW_COORDS(3)
			};

			struct fOut
			{
				float4 color : SV_Target;
			};

			VTS_UNI_SAMP
			VTS_UNI_COMMON
			VTS_UNI_CLIP

			v2f vert(vIn v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.viewPos = UnityObjectToViewPos(v.vertex);
				o.normal = UnityObjectToWorldNormal(v.normal);
				VTS_VERT_UV(v,o)
				VTS_VERT_CLIP(v,o)
				TRANSFER_SHADOW(o)
				return o;
			}

			fOut frag(v2f i)
			{
				VTS_FRAG_CLIP(i)

				fOut o;
				VTS_FRAG_COMMON(i,o)

				// shadow
				o.color.rgb *= SHADOW_ATTENUATION(i);

				// atmosphere
				float atmDensity = vtsAtmDensity(i.viewPos);
				o.color = vtsAtmColor(atmDensity, o.color);

				return o;
			}
			ENDCG
		}

		UsePass "Vts/UnlitShader/SHADOWCASTER"
	}
}

