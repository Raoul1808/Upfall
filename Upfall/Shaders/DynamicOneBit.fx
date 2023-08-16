texture ScreenTexture;

float4 BitColor1;
float4 BitColor2;

bool FlipColors;

sampler TextureSampler = sampler_state
{
    Texture = <ScreenTexture>;
};

float3 InvertArea(float3 col)
{
    return float3(1.0, 1.0, 1.0) - col;
}

float4 DynamicOneBitFunction(float2 TextureCoordinate : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(TextureSampler, TextureCoordinate);
    if (FlipColors)
        color.rgb = InvertArea(color.rgb);
    float value = (color.r + color.g + color.b) / 3;
    return value < 0.5 ? BitColor1 : BitColor2;
}

technique DynamicOneBit
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 DynamicOneBitFunction();
    }
}
