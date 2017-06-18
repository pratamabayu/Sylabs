/* BEGIN GLOBAL CONSTANT BUFFER */
float Time;

// Amount of distortion
float Amount;

// Random starting number
float Seed;
/* END GLOBAL CONSTANT BUFFER */

/* BEGIN RESOURCE AND SAMPLER */
texture2D InputMap;
sampler2D InputMapSampler
{
	Texture = InputMap;
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
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};
/* ENG SEMANTIC */

/* BEGIN VERTEX AND PIXEL SHADER */
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	output.Position = float4(input.Position.xyz, 1);
	output.UV = input.UV;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // Distortion factor
	float noiseX = Seed * Time * sin(input.UV.x * input.UV.y + Time);
	noiseX = fmod(noiseX, 8) * fmod(noiseX, 4);

	// Compute how much distortion factor will affect each texture coordinate
	float distortionX = fmod(noiseX, Amount);
	float distortionY = fmod(noiseX, Amount + 0.002f);

	// Create new texture coordinate
	float2 distortionUV = float2(distortionX, distortionY);

	// Look up a pixel in input map
	float4 color = tex2D(InputMapSampler, input.UV + distortionUV);
	color.a = 1.0f;

	return color;
}
/* END VERTEX AND PIXEL SHADER */

/* BEGIN TECHNIQUE */
technique MainTechnique
{
	pass pass0
	{
		SetVertexShader(VertexShaderFunction);
		SetPixelShader(PixelShaderFunction);
	}
}
/* END TECHNIQUE */