/*
Written by Pratama Bayu W
Email : nobley.1928@hotmail.com
*/

// Matrices
float4x4 World;
float4x4 View;
float4x4 Projection;

// Skinning
float4x4 Bones[32];

float3 CameraPosition;

// Diffuse
bool DiffuseMapEnabled;
float3 DiffuseColor;
float Alpha;

// Lighting
float3 AmbientColor;
float3 LightDirection;
float3 LightColor;
float3 EmissiveColor;
float3 SpecularColor;
float SpecularPower;

// Fogging
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
struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 Normal : NORMAL0;

	// Skinning
	float4 BoneIndices : BLENDINDICES0;
	float4 BoneWeights : BLENDWEIGHT0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float Depth : TEXCOORD3;
	float3 ViewDirection : TEXCOORD4;
};

// Entry Point
VertexShaderOutput VS(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

	// Blend between the weighted bone matrices.
	float4x4 skinTransform = float4x4(0.0f);
	int boneIndex = (int)input.BoneIndices.x;
	skinTransform += Bones[boneIndex] * input.BoneWeights.x;
        boneIndex = (int)input.BoneIndices.y;
	skinTransform += Bones[boneIndex] * input.BoneWeights.y;
        boneIndex = (int)input.BoneIndices.z;
	skinTransform += Bones[boneIndex] * input.BoneWeights.z;
        boneIndex = (int)input.BoneIndices.w;
	skinTransform += Bones[boneIndex] * input.BoneWeights.w;
	
	float4 skinPos = mul(input.Position, skinTransform);
	float3 skinNormal = mul(input.Normal, (float3x3)skinTransform);

    	float4 worldPosition = mul(skinPos, World);
    	float4 viewPosition = mul(worldPosition, View);

    	output.Position = mul(viewPosition, Projection);

	output.UV = input.UV;
	output.Normal = mul(skinNormal, (float3x3)World);

	output.Depth = output.Position.z;
	output.ViewDirection = worldPosition.xyz - CameraPosition;

    return output;
}

float4 PS(VertexShaderOutput input) : COLOR0
{	
	// Start with diffuse color
	float3 color = DiffuseColor;

	// Texture if necessary
	if(DiffuseMapEnabled)
	{
		color *= tex2D(DiffuseMapSampler, input.UV).rgb;
	}

	// Start with ambient lighting
	float3 lighting = AmbientColor;

	// Add emissive
	lighting += EmissiveColor;

	float3 lightDirection = normalize(LightDirection);
	float3 normal = normalize(input.Normal);

	// Add lambertian lighting. Formula => kdiff = max(l ? n, 0)
	lighting += saturate(dot(lightDirection, normal)) * LightColor;

	// Compute specular highlight properties
	float3 reflection = reflect(lightDirection, normal);
	float3 view = normalize(input.ViewDirection);

	// Compute specular highlight. Formula => kspec = max(r ? v, 0)^n
	float3 specular = pow(saturate(dot(reflection, view)), SpecularPower * 255.0f) * SpecularColor;

	// Calculate final color
	color = saturate(lighting) * color + specular;

	// Calculate fog
	if(FogEnabled)
	{
		float fog = saturate((input.Depth - FogStart) / (FogEnd - FogStart));
		color = lerp(color, FogColor, fog);
	}

    return float4(color, 1) * Alpha;
}

float4 PS_NoLighting(VertexShaderOutput input) : COLOR0
{	
	// Start with diffuse color
	float3 color = DiffuseColor;

	// Texture if necessary
	if(DiffuseMapEnabled)
	{
		color *= tex2D(DiffuseMapSampler, input.UV).rgb;
	}

	// Start with ambient lighting
	float3 lighting = AmbientColor;	

	// Calculate final color
	color = saturate(lighting) * color;

	// Calculate fog
	if(FogEnabled)
	{
		float fog = saturate((input.Depth - FogStart) / (FogEnd - FogStart));
		color = lerp(color, FogColor, fog);
	}

    return float4(color, 1) * Alpha;
}

// Techniques
technique MainTechnique
{
    pass pass0
	{
		SetVertexShader(VS);
		SetPixelShader(PS);
	}
}
technique NoLightingTechnique
{
    pass pass0
	{
		SetVertexShader(VS);
		SetPixelShader(PS_NoLighting);
	}
}
