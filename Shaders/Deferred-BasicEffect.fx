/* BEGIN GLOBAL CONSTANT BUFFER */
float4x4 World;
float4x4 View;
float4x4 Projection;

float4x4 LightViewProjection;

bool DiffuseMapEnabled;
float3 DiffuseColor;

float3 AmbientColor;
float3 EmissiveColor;
float3 SpecularColor;
float SpecularPower;

bool FogEnabled;
float FogStart;
float FogEnd;
float3 FogColor;
/* END GLOBAL CONSTANT BUFFER */

/* BEGIN RESOURCE AND SAMPLER */
texture2D DiffuseMap;
sampler2D DiffuseMapSampler
{
	Texture = DiffuseMap;
	Filter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Wrap;
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
	float2 UV : TEXCOORD0;
	float3 Normal : NORMAL0;
};

struct DepthNormal_VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float2 Depth : TEXCOORD1;
	float3 Normal : TEXCOORD2;
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
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

struct Main_VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float Depth : TEXCOORD1;
	float4 PositionCopy : TEXCOORD2;	
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
/* END SEMANTIC */

/* BEGIN VERTEX AND PIXEL SHADER */
/// Depth Normal
DepthNormal_VertexShaderOutput DepthNormal_VertexShaderFunction(DepthNormal_VertexShaderInput input)
{
    DepthNormal_VertexShaderOutput output = (DepthNormal_VertexShaderOutput)0;

    float4x4 viewProjection = mul(View, Projection);
	float4x4 worldViewProjection = mul(World, viewProjection);

	output.Position = mul(input.Position, worldViewProjection);
	output.UV = input.UV;
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
	// Set specular power to be in a [0..1] range. It will multiply by 255 later
	output.Normal.a = SpecularPower;

    return output;
}

/// Main
Main_VertexShaderOutput Main_VertexShaderFunction(Main_VertexShaderInput input)
{
    Main_VertexShaderOutput output = (Main_VertexShaderOutput)0;

    float4x4 worldViewProjection = mul(World, mul(View, Projection));

	output.Position = mul(input.Position, worldViewProjection);
	output.PositionCopy = output.Position;

	output.UV = input.UV;
	output.Depth = output.Position.z;

    return output;
}

float4 Main_PixelShaderFunction(Main_VertexShaderOutput input) : COLOR0
{	
	// Start with diffuse color
	float3 color = DiffuseColor;

	// Texture if necessary
	if(DiffuseMapEnabled)
	{
		float4 diffuse = tex2D(DiffuseMapSampler, input.UV);

		// Add diffuse
		color *= diffuse.rgb;
	}

	// Extract lighting value from light map
	float2 texCoord = postProjToScreen(input.PositionCopy) + HalfPixel;
	float4 light = tex2D(LightMapSampler, texCoord);

	// Calculate specular. Forumula = SpecularPower * SpecularColor
	float3 specular = (light.rgb * light.a) * SpecularColor;

	// Add ambient lighting
	light.rgb += AmbientColor;

	// Add emissive
	light.rgb += EmissiveColor;

	// Add specular
	light.rgb += specular;

	// Calculate output color
	float4 finalColor = float4(light.xyz * color, 1.0f);

	// Calculate fog
	if(FogEnabled)
	{
		float fog = saturate((input.Depth - FogStart) / (FogEnd - FogStart));
		finalColor = float4(lerp(finalColor.xyz, FogColor, fog), 1.0f);
	}

    return finalColor;
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
