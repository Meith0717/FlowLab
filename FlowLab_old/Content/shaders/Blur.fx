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

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float2 texelSize : register(c0); // The size of one texel (1.0/texture width, 1.0/texture height)

float4 BoxBlurPS(VertexShaderOutput input) : COLOR
{
    float4 color = float4(0, 0, 0, 0);
    float2 texCoord = input.TextureCoordinates;
    
    // Apply a simple box blur
    float radius = 5.0; // Adjust the radius of the blur here

    // Iterate over the box kernel
    for (float x = -radius; x <= radius; x++)
    {
        for (float y = -radius; y <= radius; y++)
        {
            float2 offset = float2(x * texelSize.x, y * texelSize.y);
            color += tex2D(SpriteTextureSampler, texCoord + offset);
        }
    }

    // Normalize the color by the number of samples (box kernel size)
    color /= ((radius * 2 + 1) * (radius * 2 + 1));

    return color * input.Color;
}

technique BlurDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL BoxBlurPS();
    }
};
