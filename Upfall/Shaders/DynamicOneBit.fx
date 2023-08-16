texture ScreenTexture;

float2 CirclePos;
float CircleRadius;

float4 BitColor1;
float4 BitColor2;

sampler TextureSampler = sampler_state
{
    Texture = <ScreenTexture>;
};

float circle(float radius, float2 circlePos, float2 uv)
{
    float value = distance(uv, circlePos);
    return step(value, radius);
}

float4 DynamicOneBitFunction(float2 pixelCoord : SV_Position, float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 texColor = tex2D(TextureSampler, texCoord);
    float grayscale = 0.0;
    if (circle(CircleRadius, CirclePos, pixelCoord) > 0.5)
        grayscale = 1.0 - texColor.b;
    else
        grayscale = texColor.r;
    return grayscale < 0.5 ? BitColor1 : BitColor2;
}

technique DynamicOneBit
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 DynamicOneBitFunction();
    }
}
