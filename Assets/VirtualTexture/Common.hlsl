#ifndef VIRTUAL_TEXTURE_COMMON_INCLUDED
#define VIRTUAL_TEXTURE_COMMON_INCLUDED

int3 calcPage(
    const float2 vUv,
    const float vPageSize,
    const float vPageResolution,
    const float vMaxMipmapLevel,
    const float vMinMipmapLevel
)
{
    float2 Page = floor(vUv * vPageSize);
    const float2 Coord = vUv * vPageResolution;
    const float2 Dx = ddx(Coord);
    const float2 Dy = ddy(Coord);
    float Mip = clamp(0.5 * log2(max(dot(Dx, Dx), dot(Dy, Dy))), vMinMipmapLevel, vMaxMipmapLevel);
    Mip = floor(Mip);
    Page = floor(Page / exp2(Mip));
    int Col = int(Page.x);
    int Row = int(Page.y);
    return int3(Row, Col, Mip);
}

#endif