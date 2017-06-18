/*
Written by Pratama Bayu W
Email : nobley.1928@live.com
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

// Lighting
bool LightMapEnabled;

// Fog
bool FogEnabled;
float FogStart;
float FogEnd;
float3 FogColor;
/* END GLOBAL CONSTANT BUFFER */

/* BEGIN RESOURCE AND SAMPLER */
// Resources
texture2D DiffuseMap;
texture2D LightMap;

// Samplers
sampler2D DiffuseMapSampler
{
	Texture = DiffuseMap;
	Filter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
	AddressW = Clamp;
};
sampler2D LightMapSampler
{
	Texture = LightMap;
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
	float2 UV0 : TEXCOORD0;
	float2 UV1 : TEXCOORD1;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 UV0 : TEXCOORD0;
	float2 UV1 : TEXCOORD1;
	float Depth : TEXCOORD2;	
};
/* ENG SEMANTIC */

/* BEGIN VERTEX AND PIXEL SHADER */
VertexShaderOutput VS(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);

    output.Position = mul(viewPosition, Projection);

	output.UV0 = input.UV0;
	output.UV1 = input.UV1;

	output.Depth = output.Position.z;

    return output;
}

float4 PS(VertexShaderOutput input) : COLOR0
{	
	// Start with diffuse color
	float4 color = float4(DiffuseColor, 1);

	// Texture if necessary
	if(DiffuseMapEnabled)
	{
		color *= tex2D(DiffuseMapSampler, input.UV0);
	}

	if (LightMapEnabled)
	{
		float4 light = tex2D(LightMapSampler, input.UV1);
		color.rgb *= light.rgb * 2.0f;
		color.a *= light.a;
	}

	// Calculate fog
	if(FogEnabled)
	{
		float fog = saturate((input.Depth - FogStart) / (FogEnd - FogStart));
		color = lerp(color, float4(FogColor, 1), fog);
	}

    return color;
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
/* END TECHNIQUE */