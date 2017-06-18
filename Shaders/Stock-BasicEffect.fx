/*
Written by Pratama Bayu Widagdo
Email : nobley.1928@hotmail.com
*/

// Constants
float4x4 World;
float4x4 View;
float4x4 Projection;

// Texture
float3 DiffuseColor;
float Alpha;

// Lighting

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

struct VS_COLOR_TEXTURE_IN
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	float2 UV : TEXCOORD0;
};

struct PS_COLOR_TEXTURE_IN
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	float2 UV : TEXCOORD0;
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

PS_COLOR_IN VS_COLOR( VS_COLOR_IN input )
{
	PS_COLOR_IN output = (PS_COLOR_IN)0;
	
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);

	output.Color = input.Color;
	
	return output;
}

float4 PS_COLOR( PS_COLOR_IN input ) : COLOR0
{
	return input.Color * Alpha;
}

PS_COLOR_TEXTURE_IN VS_COLOR_TEXTURE( VS_COLOR_TEXTURE_IN input )
{
	PS_COLOR_TEXTURE_IN output = (PS_COLOR_TEXTURE_IN)0;
	
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);

	output.Color = input.Color;
	output.UV = input.UV;
	
	return output;
}

float4 PS_COLOR_TEXTURE( PS_COLOR_TEXTURE_IN input ) : COLOR0
{
	// Get diffuse
	float4 diffuse = tex2D(DiffuseMapSampler, input.UV);

	// Start with diffuse color
	float3 color = DiffuseColor * input.Color.rgb;

	// Add diffuse texture
	color *= diffuse.rgb;

	return float4(color, 1) * Alpha;
}

float4 PS_COLOR_NOTEXTURE( PS_COLOR_TEXTURE_IN input ) : COLOR0
{
	// Start with diffuse color
	float3 color = DiffuseColor * input.Color.rgb;

	return float4(color, 1) * Alpha;
}

PS_TEXTURE_IN VS_TEXTURE( VS_TEXTURE_IN input )
{
	PS_TEXTURE_IN output = (PS_TEXTURE_IN)0;
	
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);

	output.UV = input.UV;
	
	return output;
}

float4 PS_TEXTURE( PS_TEXTURE_IN input ) : COLOR0
{
	// Get diffuse
	float4 diffuse = tex2D(DiffuseMapSampler, input.UV);

	// Start with diffuse color
	float3 color = DiffuseColor;

	// Add diffuse texture
	color *= diffuse.rgb;

	return float4(color, 1) * Alpha;
}

float4 PS_NOTEXTURE( PS_TEXTURE_IN input ) : COLOR0
{
	// Start with diffuse color
	float3 color = DiffuseColor;

	return float4(color, 1) * Alpha;
}

technique VertexColor
{
	pass pass0
	{
		SetVertexShader(VS_COLOR);
		SetPixelShader(PS_COLOR);
	}
}

technique VertexColorTexture
{
	pass pass0
	{
		SetVertexShader(VS_COLOR_TEXTURE);
		SetPixelShader(PS_COLOR_TEXTURE);
	}
}

technique VertexColorNoTexture
{
	pass pass0
	{
		SetVertexShader(VS_COLOR_TEXTURE);
		SetPixelShader(PS_COLOR_NOTEXTURE);
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

technique VertexNoTexture
{
	pass pass0
	{
		SetVertexShader(VS_TEXTURE);
		SetPixelShader(PS_NOTEXTURE);
	}
}