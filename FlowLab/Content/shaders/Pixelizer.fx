#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

float2 pixelSize;

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    // Get the current texture coordinates
    float2 uv = input.TextureCoordinates;

    // Calculate the new texture coordinates for pixelation
    uv = floor(uv / pixelSize) * pixelSize;

    // Sample the texture using the new pixelated coordinates
    float4 color = tex2D(SpriteTextureSampler, uv);

    return color * input.Color;
}

technique PixelateEffect
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
