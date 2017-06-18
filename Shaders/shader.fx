/*
Written by Pratama Bayu Widagdo
Email : nobley.1928@live.com
*/

// Constants
float4x4 World;
float4x4 View;
float4x4 Projection;

float3 DiffuseColor;
float Alpha;

// Resources and Samplers
texture2D DiffuseMap;
sampler2D DiffuseMapSampler 
{
	Texture = DiffuseMap;
	Filter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
	AddressW = Clamp;
};

// Semantics
struct VS_COLOR_IN
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
};

struct PS_COLOR_IN
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
};

struct VS_TEXTURE_IN
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

struct PS_TEXTURE_IN
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

struct PS_OUT
{
	float4 Color : COLOR0;
};

float2 halfPixel(float width, float height)
{
	return 0.5f / float2(width, height);
}

// Main Functions
PS_COLOR_IN VS_COLOR(VS_COLOR_IN input)
{
	PS_COLOR_IN output = (PS_COLOR_IN)0;
	
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);

	output.Color = input.Color;
	
	return output;
}

PS_OUT PS_COLOR(PS_COLOR_IN input)
{
	PS_OUT output = (PS_OUT)0;

	output.Color = input.Color * Alpha;

	return output;
}

PS_TEXTURE_IN VS_TEXTURE(VS_TEXTURE_IN input)
{
	PS_TEXTURE_IN output = (PS_TEXTURE_IN)0;
	
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);

	output.UV = input.UV;
	
	return output;
}

float4 PS_TEXTURE(PS_TEXTURE_IN input) : COLOR0
{
	// Get diffuse
	float4 diffuse = tex2D(DiffuseMapSampler, input.UV * halfPixel(800, 800));
		
	// Alpha masked
	if (diffuse.a < Alpha)
	{
		discard;
	}

	// Start with diffuse color
	float3 color = DiffuseColor;

	// Add diffuse texture
	color *= diffuse.rgb;

	// Apply final color
	return float4(color, 1) * Alpha;
}

// Techniques
technique VertexColor
{
	pass pass0
	{
		SetVertexShader(VS_COLOR);
		SetPixelShader(PS_COLOR);
	}
}

technique VertexTexture
{
	pass pass0
	{
		SetVertexShader(VS_TEXTURE);
		SetPixelShader(PS_TEXTURE);
	}
}