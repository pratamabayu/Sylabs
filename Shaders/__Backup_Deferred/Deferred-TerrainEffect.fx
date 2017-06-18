/* BEGIN GLOBAL CONSTANT BUFFER */
float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

// Deferred rendering
float4x4 LightViewProjection;

// Lighting
float3 AmbientColor;
float3 DiffuseColor0;
float3 DiffuseColor1;
float3 DiffuseColor2;
float3 DiffuseColor3;

// Fog properties
bool FogEnabled;
float FogStart;
float FogEnd;
float3 FogColor;

// Detail distance
float DetailDistance;

// Tiling
float2 Tiling0;
float2 Tiling1;
float2 Tiling2;
float2 Tiling3;
float2 DetailTiling;

// Editing
bool EditorMode;
float3 MousePositionOnTerrain;
float Surface;
/* END GLOBAL CONSTANT BUFFER */

/* BEGIN RESOURCE AND SAMPLER */
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

texture2D LightMap;
sampler2D LightMapSampler
{
	Texture = LightMap;
	Filter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
	AddressW = Clamp;
};
/* END RESOURCE AND SAMPLER */

/* BEGIN INCLUDE */
// Calculate the size of one half of a pixel, to convert
// between texels and pixels from map size
float2 HalfPixel;

// Calculate the 2D screen position of a 3D position
float2 postProjToScreen(float4 position)
{
	float2 screenPosition = position.xy / position.w;
	return 0.5f * (float2(screenPosition.x, -screenPosition.y) + 1.0f);
}
/* END INCLUDE */

/* BEGIN FUNCTION */
/* END FUNCTION */

/* BEGIN SEMANTIC */
struct DepthNormal_VertexShaderInput
{
    float4 Position : POSITION0;
	float3 Normal : NORMAL0;
};

struct DepthNormal_VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 Depth : TEXCOORD0;
	float3 Normal : TEXCOORD1;
};

// We render to two targets simultaneously, so we can't
// simply return a float4 from the pixel shader
struct DepthNormal_PixelShaderOutput
{
	float4 Normal : COLOR0;
	float4 Depth : COLOR1;
};

struct Main_VertexShaderInput
{
    float3 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 Normal : NORMAL0;
};

struct Main_VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float Depth : TEXCOORD1;
	// Terrain editing
	float Distance : TEXCOORD2;
	float4 PositionCopy : TEXCOORD3;	
};

struct Shadow_VertexShaderInput
{
    float4 Position : POSITION0;
};

struct Shadow_VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 Depth : TEXCOORD0;
};
/* ENG SEMANTIC */

/* BEGIN VERTEX AND PIXEL SHADER */
/// Depth Normal
DepthNormal_VertexShaderOutput DepthNormal_VertexShaderFunction(DepthNormal_VertexShaderInput input)
{
    DepthNormal_VertexShaderOutput output = (DepthNormal_VertexShaderOutput)0;

    float4x4 viewProjection = mul(View, Projection);
	float4x4 worldViewProjection = mul(World, viewProjection);

	output.Position = mul(input.Position, worldViewProjection);
	output.Normal = mul(input.Normal, (float3x3)World);

	// Position's z and w components correspond to the distance
	// from camera and distance of the far plane respectively
	output.Depth.xy = output.Position.zw;

    return output;
}

DepthNormal_PixelShaderOutput DepthNormal_PixelShaderFunction(DepthNormal_VertexShaderOutput input)
{
    DepthNormal_PixelShaderOutput output = (DepthNormal_PixelShaderOutput)0;
	
	// Depth is stored as distance from camera / far plane distance
	// to get value between 0 and 1
	float depth = input.Depth.x / input.Depth.y;
	output.Depth = float4(depth, depth, depth, depth);

	// Normal map simply stores X, Y and Z components of normal
	// shifted from (-1 to 1) range to (0 to 1) range
	output.Normal.xyz = (normalize(input.Normal).xyz / 2.0f) + 0.5f;

	// Other components must be initialized to compile
	output.Depth.a = 1.0f;
	output.Normal.a = 1.0f;

    return output;
}

/// Main
Main_VertexShaderOutput Main_VertexShaderFunction(Main_VertexShaderInput input)
{
    Main_VertexShaderOutput output = (Main_VertexShaderOutput)0;

    float4x4 worldViewProjection = mul(mul(World, View), Projection);
	output.Position = mul(float4(input.Position, 1.0f), worldViewProjection);
	output.PositionCopy = output.Position;

	output.UV = input.UV;
	output.Depth = output.Position.z;

	// Terrain editing
	if (EditorMode)
	{
		output.Distance = distance(input.Position.xz, MousePositionOnTerrain.xz);
	}

    return output;
}

float4 Main_PixelShaderFunction(Main_VertexShaderOutput input) : COLOR0
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

	// Extract lighting value from light map
	float2 texCoord = postProjToScreen(input.PositionCopy) + HalfPixel;
	float3 lighting = tex2D(LightMapSampler, texCoord).rgb;

	// Add ambient lighting
	lighting += AmbientColor;

	// Calculate output color
	float3 finalColor = detail * color * lighting;

	// Add a small constant to avoid dark areas
	finalColor.rgb += color * 0.1f;

	// Calculate fog
	if(FogEnabled)
	{
		float fog = saturate((input.Depth - FogStart) / (FogEnd - FogStart));
		finalColor = lerp(finalColor, FogColor, fog);
	}

	// Calculate cursor for editing
	if (EditorMode)
	{
		if (input.Distance < Surface)
		{
			float distanceFromCircle = abs(Surface - input.Distance); 
			float alpha = saturate((Surface / 4.0f) - distanceFromCircle);
			float4 color = float4(0.25f, 1.0f, 0.0f, 1.0f);
			finalColor.rgb += color.rgb * alpha;	
		}
	}

    return float4(finalColor, 1.0f);
}

/// Shadow
Shadow_VertexShaderOutput Shadow_VertexShaderFunction(Shadow_VertexShaderInput input)
{
	Shadow_VertexShaderOutput output = (Shadow_VertexShaderOutput)0;

	// Calculate screen space position
	float4x4 worldViewProjection = mul(World, LightViewProjection);
	float4 position = mul(input.Position, worldViewProjection);

	// Clamp to the near plane
	position.z = max(position.z, 0.0f);

	output.Position = position;
	output.Depth = output.Position.zw;

	return output;
}

float4 Shadow_PixelShaderFunction(Shadow_VertexShaderOutput input) : COLOR0
{
	// Determine the depth of this vertex
	float depth = input.Depth.x / input.Depth.y;

    return float4(depth, 0.0f, 0.0f, 1.0f); 
}
/* END VERTEX AND PIXEL SHADER */

/* BEGIN TECHNIQUE */
technique DepthNormalTechnique
{
	pass Pass0
    {
		SetVertexShader(DepthNormal_VertexShaderFunction);
		SetPixelShader(DepthNormal_PixelShaderFunction);
    }
}

technique MainTechnique
{
	pass Pass0
    {
		SetVertexShader(Main_VertexShaderFunction);
		SetPixelShader(Main_PixelShaderFunction);
    }
}

technique ShadowTechnique
{
	pass Pass0
    {
		SetVertexShader(Shadow_VertexShaderFunction);
		SetPixelShader(Shadow_PixelShaderFunction);
    }
}
/* END TECHNIQUE */