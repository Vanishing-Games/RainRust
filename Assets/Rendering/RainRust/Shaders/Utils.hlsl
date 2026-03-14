#ifndef GI_LIGHT_2D_INCLUDED
#define GI_LIGHT_2D_INCLUDED

// =======================================================================

#pragma multi_compile_local STEPS_4 STEPS_6 STEPS_8 STEPS_12 STEPS_16

#define AMBIENT		float3(0, 0, 0)

#if defined STEPS_4
#define STEPS		4
#elif defined STEPS_6
#define STEPS		6
#elif defined STEPS_8
#define STEPS		8
#elif defined STEPS_12
#define STEPS		12
#elif defined STEPS_16
#define STEPS		16
#else
#define STEPS		16
#endif

// =======================================================================

struct VertInput
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct FragInput
{
    float4 vertex : SV_POSITION;
    float2 uv : TEXCOORD0;
};

// =======================================================================

FragInput VertDefault(VertInput v)
{
    FragInput o;
    o.vertex = v.vertex;
    o.uv = v.uv;

    return o;
}

float Falloff(const in float2 dist, const in float factor)
{
    return 1 - clamp(length(dist) / factor, 0, 1);
}

bool NotUVSpace(const in float2 uv)
{
    const float2 uvAbs = abs(uv - float2(.5, .5));
    return uvAbs.x > .5 || uvAbs.y > .5;
}

bool IsOutlinePixel(in const sampler2D tex, in const float2 uv, in const float2 texel)
{
    return tex2D(tex, uv).r * 
        (tex2D(tex, uv + float2(texel.x, 0)).r +
        tex2D(tex, uv - float2(texel.x, 0)).r +
        tex2D(tex, uv + float2(0, texel.y)).r +
        tex2D(tex, uv - float2(0, texel.y)).r)
    > 0;
}

float Random(in const float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
}

#endif
