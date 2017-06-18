/*
Written by Pratama Bayu Widagdo
Email : nobley.1928@hotmail.com
*/

// Constants
// Matrices
float4x4 World;
float4x4 View;
float4x4 Projection;

// Fog
bool FogEnabled;
float FogStart;
float FogEnd;
float3 FogColor;

// Resources
texture2D DiffuseMap;

// Samplers
sampler2D DiffuseMapSampler
{
	Texture = DiffuseMap;
	Filter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
	AddressW = Clamp;
};

// Semantics
struct VS_IN
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	float2 UV : TEXCOORD0;
};

struct PS_IN
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	float2 UV : TEXCOORD0;
	float Depth : TEXCOORD1;
};

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;
	
	float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);

    output.Position = mul(viewPosition, Projection);
	output.Color = input.Color;
	output.UV = input.UV;
	output.Depth = output.Position.z;

	return output;
}

float4 PS(PS_IN input) : COLOR0
{
	// Start with diffuse map
	float4 color = tex2D(DiffuseMapSampler, input.UV);

	// Blend with vertex color
	color *= input.Color;

	// Add fog
	if(FogEnabled)
	{
		float fog = saturate((input.Depth - FogStart) / (FogEnd - FogStart));
		color = float4(lerp(color.rgb, FogColor, fog), color.a);
	}
	
	return color;
}

technique MainTechnique
{
	pass Pass0
	{
		SetVertexShader(VS);
		SetPixelShader(PS);
	}
}