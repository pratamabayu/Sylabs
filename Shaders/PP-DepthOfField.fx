/* BEGIN GLOBAL CONSTANT BUFFER */
// Camera far plane
float MaxDepth;
// Distance at which blur starts
float BlurStart;
// Distance at which scene is fully blurred
float BlurEnd;
/* END GLOBAL CONSTANT BUFFER */

/* BEGIN RESOURCE AND SAMPLER */
// Blur
texture2D InputMap;
sampler2D InputMapSampler
{
	Texture = InputMap;
	Filter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
	AddressW = Clamp;
};
// Unblur
texture2D OriginalMap;
sampler2D OriginalMapSampler
{
	Texture = OriginalMap;
	Filter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
	AddressW = Clamp;
};
// Depth
texture2D DepthMap;
sampler2D DepthMapSampler
{
	Texture = DepthMap;
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
    // Determine depth
	float depth = tex2D(DepthMapSampler, input.UV).r * MaxDepth;

	// Get blurred and unblurred render of scene
	float4 unblurred = tex2D(OriginalMapSampler, input.UV);
	float4 blurred = tex2D(InputMapSampler, input.UV);

	// Determine blur amount (similar to fog calculation)
	float blurAmount = clamp((depth - BlurStart) / (BlurEnd - BlurStart), 0, 1);

	// Blend between unblurred and blurred images
	float4 mix = lerp(unblurred, blurred, blurAmount);

	return mix;
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