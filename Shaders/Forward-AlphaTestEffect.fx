/*
Written by Pratama Bayu W
Email : nobley.1928@hotmail.com
*/

/* BEGIN GLOBAL CONSTANT BUFFER */
// Matrices
float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;

// Diffuse
bool DiffuseMapEnabled;
float3 DiffuseColor;
float ReferenceAlpha;
float Alpha;

// Lighting
float3 AmbientColor;
float3 LightDirection;
float3 LightColor;
float3 EmissiveColor;
float3 SpecularColor;
float SpecularPower;

// Fog
bool FogEnabled;
float FogStart;
float FogEnd;
float3 FogColor;
/* END GLOBAL CONSTANT BUFFER */

/* BEGIN RESOURCE AND SAMPLER */
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
/* END RESOURCE AND SAMPLER */

/* BEGIN INCLUDE */
/* END INCLUDE */

/* BEGIN FUNCTION */
/* END FUNCTION */

/* BEGIN SEMANTIC */
struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float Depth : TEXCOORD2;	
	float3 ViewDirection : TEXCOORD3;
};
/* ENG SEMANTIC */

/* BEGIN VERTEX AND PIXEL SHADER */
VertexShaderOutput VS(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);

    output.Position = mul(viewPosition, Projection);

	output.UV = input.UV;
	output.Normal = mul(input.Normal, (float3x3)World);

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
		float4 diffuse = tex2D(DiffuseMapSampler, input.UV);
		if (diffuse.w < ReferenceAlpha)
		{
			discard;
		}

		color *= diffuse.rgb;
	}

	// Start with ambient lighting
	float3 lighting = AmbientColor;

	// Add emissive
	lighting += EmissiveColor;

	float3 lightDirection = normalize(LightDirection);
	float3 normal = normalize(input.Normal);

	// Add lambertian lighting. Formula => kdiff = max(l ??? n, 0)
	lighting += saturate(dot(lightDirection, normal)) * LightColor;

	// Compute specular highlight properties
	float3 reflection = reflect(lightDirection, normal);
	float3 view = normalize(input.ViewDirection);

	// Compute specular highlight. Formula => kspec = max(r ??? v, 0)^n
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
		float4 diffuse = tex2D(DiffuseMapSampler, input.UV);
		if (diffuse.w < ReferenceAlpha)
		{
			discard;
		}

		color *= diffuse.rgb;
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
/* END VERTEX AND PIXEL SHADER */

/* BEGIN TECHNIQUE */
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
/* END TECHNIQUE */