texture ScreenTexture;

float4 BitColor1;
float4 BitColor2;

sampler TextureSampler = sampler_state
{
    Texture = <ScreenTexture>;
};

float4 DynamicOneBitFunction(float2 TextureCoordinate : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(TextureSampler, TextureCoordinate);
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