/*
Written by Pratama Bayu W
Email : nobley.1928@hotmail.com
*/

// CONSTANTS
// Matrices
float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

// Diffuse
float3 DiffuseColor0;
float3 DiffuseColor1;
float3 DiffuseColor2;
float3 DiffuseColor3;

// Tiling
float2 Tiling0;
float2 Tiling1;
float2 Tiling2;
float2 Tiling3;

// Details
float DetailDistance;
float2 DetailTiling;

// Lighting
float3 LightDirection;
float3 LightColor;
float3 AmbientColor;

// Foging
bool FogEnabled;
float FogStart;
float FogEnd;
float3 FogColor;

// Editing
bool EditorMode;
float3 MousePositionOnTerrain;
float Surface;

// RESOURCES AND SAMPLERS
texture2D DiffuseMap0;
sampler2D DiffuseMap0Sampler 
{
	Texture = DiffuseMap0;
	Filter = Anisotropic;
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Wrap;
};

texture2D DiffuseMap1;
sampler2D DiffuseMap1Sampler 
{
	Texture = DiffuseMap1;
	Filter = Anisotropic;
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Wrap;
};

texture2D DiffuseMap2;
sampler2D DiffuseMap2Sampler 
{
	Texture = DiffuseMap2;
	Filter = Anisotropic;
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Wrap;
};

texture2D DiffuseMap3;
sampler2D DiffuseMap3Sampler 
{
	Texture = DiffuseMap3;
	Filter = Anisotropic;
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Wrap;
};

texture2D DetailMap;
sampler2D DetailMapSampler 
{
	Texture = DetailMap;
	Filter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Wrap;
};

texture2D BlendMap;
sampler2D BlendMapSampler 
{
	Texture = BlendMap;
	Filter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
	AddressW = Clamp;
};

// SEMANTICS
struct VertexShaderInput
{
    	float3 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;	
	// Fogging
	float Depth : TEXCOORD1;
	// Editing
	float Distance : TEXCOORD2;
	// Normal
	float3 Normal : TEXCOORD3;
};

// ENTRY POINTS
VertexShaderOutput VS(VertexShaderInput input)
{
    	VertexShaderOutput output = (VertexShaderOutput)0;

    	float4x4 worldViewProjection = mul(mul(World, View), Projection);
	output.Position = mul(float4(input.Position, 1.0f), worldViewProjection);

	output.UV = input.UV;
	output.Normal = mul(input.Normal, (float3x3)WorldInverseTranspose);

	output.Depth = output.Position.z;

	// Terrain editing
	if (EditorMode)
	{
		output.Distance = distance(input.Position.xz, MousePositionOnTerrain.xz);
	}

    	return output;
}

float4 PS(VertexShaderOutput input) : COLOR0
{
	// Get terrain colors
	float3 base = DiffuseColor0;
	base *= tex2D(DiffuseMap0Sampler, input.UV * Tiling0).rgb;
	float3 rTex = DiffuseColor1;
	rTex *= tex2D(DiffuseMap1Sampler, input.UV * Tiling1).rgb;
	float3 gTex = DiffuseColor2;
	gTex *= tex2D(DiffuseMap2Sampler, input.UV * Tiling2).rgb;
	float3 bTex = DiffuseColor3;
	bTex *= tex2D(DiffuseMap3Sampler, input.UV * Tiling3).rgb;

	// Calculate multitexture
	float3 blend = tex2D(BlendMapSampler, input.UV).rgb;
	float adjustBlend = clamp(1.0f - blend.r - blend.g - blend.b, 0.0f, 1.0f);
	float3 color = float3(adjustBlend, adjustBlend, adjustBlend);
	color *= base;
	color += blend.r * rTex + blend.g * gTex + blend.b * bTex;

	// Calculate detail texture
	float3 detail = tex2D(DetailMapSampler, input.UV * DetailTiling).rgb;
	float detailAmt = input.Depth / DetailDistance;
	detail = lerp(detail, float3(1.0f, 1.0f, 1.0f), clamp(detailAmt, 0.0f, 1.0f));

	// Start with ambient lighting
	float3 lighting = AmbientColor;

	float3 lightDirection = normalize(LightDirection);
	float3 normal = normalize(input.Normal);

	// Add lambertian lighting. Formula => kdiff = max(l ? n, 0)
	lighting += saturate(dot(lightDirection, normal)) * LightColor;

	// Calculate final color
	color = detail * color * saturate(lighting);

	// Calculate fog
	if (FogEnabled)
	{
		float fog = saturate((input.Depth - FogStart) / (FogEnd - FogStart));
		color = lerp(color, FogColor, fog);
	}

	// Terrain editing
	if (EditorMode)
	{
		if (input.Distance < Surface)
		{
			float distanceFromCircle = abs(Surface - input.Distance); 
			float alpha = saturate((Surface / 4.0f) - distanceFromCircle);
			float4 mask = float4(0.25f, 1, 0, 1);
			color.rgb += mask.rgb * alpha;	
		}
	}

    	return float4(color, 1);
}

float4 PS_NoLighting(VertexShaderOutput input) : COLOR0
{
	// Get terrain colors
	float3 base = DiffuseColor0;
	base *= tex2D(DiffuseMap0Sampler, input.UV * Tiling0).rgb;
	float3 rTex = DiffuseColor1;
	rTex *= tex2D(DiffuseMap1Sampler, input.UV * Tiling1).rgb;
	float3 gTex = DiffuseColor2;
	gTex *= tex2D(DiffuseMap2Sampler, input.UV * Tiling2).rgb;
	float3 bTex = DiffuseColor3;
	bTex *= tex2D(DiffuseMap3Sampler, input.UV * Tiling3).rgb;

	// Calculate multi texture
	float3 blend = tex2D(BlendMapSampler, input.UV).rgb;
	float adjustBlend = clamp(1.0f - blend.r - blend.g - blend.b, 0.0f, 1.0f);
	float3 color = float3(adjustBlend, adjustBlend, adjustBlend);
	color *= base;
	color += blend.r * rTex + blend.g * gTex + blend.b * bTex;

	// Calculate detail texture
	float3 detail = tex2D(DetailMapSampler, input.UV * DetailTiling).rgb;
	float detailAmt = input.Depth / DetailDistance;
	detail = lerp(detail, float3(1.0f, 1.0f, 1.0f), clamp(detailAmt, 0.0f, 1.0f));

	// Start with ambient lighting
	float3 lighting = AmbientColor;	

	// Calculate final color
	color = detail * color * saturate(lighting);

	// Calculate fog
	if (FogEnabled)
	{
		float fog = saturate((input.Depth - FogStart) / (FogEnd - FogStart));
		color = lerp(color, FogColor, fog);
	}

	// Terrain editing
	if (EditorMode)
	{
		if (input.Distance < Surface)
		{
			float distanceFromCircle = abs(Surface - input.Distance); 
			float alpha = saturate((Surface / 4.0f) - distanceFromCircle);
			float4 mask = float4(0.25f, 1, 0, 1);
			color.rgb += mask.rgb * alpha;	
		}
	}

    return float4(color, 1);
}

technique MainTechnique
{
    	pass Pass0
    	{
        	SetVertexShader(VS);
			SetPixelShader(PS);
    	}
}
technique NoLightingTechnique
{
    	pass Pass0
    	{
        	SetVertexShader(VS);
			SetPixelShader(PS_NoLighting);
    	}
}
