#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix View;
matrix Projection;

struct VertexShaderInput
{
    float4 Position : POSITION0;   // The local quad position (-1 to 1)
    float2 UV : TEXCOORD0;         // The UV coordinates
};

// This data comes from your second VertexBuffer (the instances)
struct InstanceInput
{
    float4 InstancePosition : POSITION1; 
    float4 InstanceColor : COLOR1;       
    float InstanceSize : TEXCOORD1;         
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 UV : TEXCOORD0;
};

VertexShaderOutput MainVS(VertexShaderInput input, InstanceInput instance)
{
    VertexShaderOutput output;

    // Billboard calculation: Extract Right and Up vectors from the View Matrix
    // This ensures the "circle" quad always faces the camera
    float3 worldRight = float3(View._11, View._21, View._31);
    float3 worldUp = float3(View._12, View._22, View._32);
    
    // Position the quad in the world based on the particle instance
    float3 worldPos = instance.InstancePosition.xyz 
                    + (worldRight * input.Position.x * (instance.InstanceSize / 2))
                    + (worldUp * input.Position.y * (instance.InstanceSize / 2));

    float4 viewPos = mul(float4(worldPos, 1.0), View);
    output.Position = mul(viewPos, Projection);
    
    output.UV = input.UV;
    output.Color = instance.InstanceColor;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float distSq = dot(input.UV, input.UV);
    
    if (distSq > 1.0) 
        discard;
    
    float3 color = input.Color.rgb * (1 - smoothstep(0.9, 1.0, distSq));
    return float4(color, 1);
}

technique ParticleDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};