#if OPENGL
#define SV_POSITION POSITION
#define PS_SHADERMODEL ps_3_0
#else
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#define RADIUS 20
#define KERNEL_LENGTH (RADIUS * 2 + 1)

Texture2D SpriteTexture;

sampler2D TexSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

float2 texelSize;
float kernel[KERNEL_LENGTH];

struct PSInput
{
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

// Horizontal blur pass
float4 PS_BlurH(PSInput IN) : SV_Target
{
    float4 sum = float4(0, 0, 0, 0);
    int idx = 0;
    for (int x = -RADIUS; x <= RADIUS; x++)
    {
        float2 offset = float2(x, 0) * texelSize;
        sum += tex2D(TexSampler, IN.TexCoord + offset) * kernel[idx++];
    }
    return sum;
}

// Vertical blur pass
float4 PS_BlurV(PSInput IN) : SV_Target
{
    float4 sum = float4(0, 0, 0, 0);
    int idx = 0;
    for (int y = -RADIUS; y <= RADIUS; y++)
    {
        float2 offset = float2(0, y) * texelSize;
        sum += tex2D(TexSampler, IN.TexCoord + offset) * kernel[idx++];
    }
    return sum;
}

technique GaussianBlurH
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL PS_BlurH();
    }

};

technique GaussianBlurV
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL PS_BlurV();
    }

};
