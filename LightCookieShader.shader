Shader "Unlit/LightCookieShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "bump" {}
    }
    
    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    float4 _MainTex_ST;
    float4 _MainTex_TexelSize;
    float4 pixel_col;
    
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

//##############################AspectFix Pass 0##############################
    
    v2f vert_fixaspect (appdata v)
    {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        
        float width = _MainTex_TexelSize.z;
        float height = _MainTex_TexelSize.w;

        //o.uv.x = 1 - o.uv.x;
        o.uv.x *= 3.0f;
        o.uv.x = o.uv.x - 1.0f;
        
        o.uv.y = o.uv.y * width/height - (width/height - 1.0f)/2.0f;
        o.uv.y *= 3.0f;
        o.uv.y = o.uv.y - 1.0f;
        return o;
    }

    float4 fragment_fixaspect (v2f i) : SV_Target
    {
        pixel_col = tex2D(_MainTex,i.uv);

        if(     i.uv.y > _MainTex_TexelSize.x * (_MainTex_TexelSize.z-0.5) ||
                i.uv.y < _MainTex_TexelSize.x * (1-0.5)||
                i.uv.x > _MainTex_TexelSize.y * (_MainTex_TexelSize.w-0.5) ||
                i.uv.x < _MainTex_TexelSize.y * (1-0.5) )
                {
                    pixel_col.rgb = 0;
                }
        
        return pixel_col;
    }
    
//##############################Boxfilter Pass 1##############################
    
    half4 frag_boxfilter (v2f i) : SV_Target
    {
        
        float4 pix1 = tex2D(_MainTex, i.uv + float2(-_MainTex_TexelSize.x, -_MainTex_TexelSize.y));
        float4 pix2 = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x, -_MainTex_TexelSize.y));
        float4 pix3 = tex2D(_MainTex, i.uv + float2(-_MainTex_TexelSize.x, _MainTex_TexelSize.y));
        float4 pix4 = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y));

        pixel_col = half4(0.25f * (pix1 + pix2 + pix3 + pix4));
        
        return pixel_col;
    }

//##############################Greyscale Pass 2##############################
    
    half4 frag_greyscale (v2f i) : SV_Target
    {
        pixel_col.rgb = tex2D(_MainTex, i.uv).rgb;
        pixel_col.a = (tex2D(_MainTex, i.uv).r * 0.2126
                    + tex2D(_MainTex, i.uv).g * 0.7152
                    + tex2D(_MainTex, i.uv).b * 0.0722);
        
        return pixel_col;
    }

//##############################Brightness increase Pass 3##################
    half4 frag_incbright (v2f i) : SV_Target
    {
        pixel_col.rgba = tex2D(_MainTex, i.uv).rgba + 0.001;
        
        return pixel_col;
    }
    
//##############################Nothing Pass 5##############################

    v2f vert_nothing (appdata v)
    {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        return o;
    }

    float4 frag_nothing (v2f i) : SV_Target
    {
        return tex2D(_MainTex, i.uv);
    }
    
    ENDCG
    
    SubShader
    {
        Pass
        {
            Name "FixAspect"
            ZTest Always
            Cull Off 
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert_fixaspect
            #pragma fragment fragment_fixaspect
            ENDCG
        }
        Pass
        {
            Name "BoxFilter"
            ZTest Always 
            Cull Off 
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert_nothing
            #pragma fragment frag_boxfilter
            ENDCG
        }
        Pass
        {
            Name "GreyScale"
            ZTest Always 
            Cull Off 
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert_nothing
            #pragma fragment frag_greyscale
            ENDCG
        }
        Pass
        {
            Name "IncBrightness"
            ZTest Always 
            Cull Off 
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert_nothing
            #pragma fragment frag_incbright
            ENDCG
        }
        Pass
        {
            Name "Nothing"
            ZTest Always 
            Cull Off 
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert_nothing
            #pragma fragment frag_nothing
            ENDCG
        }
    }
}
