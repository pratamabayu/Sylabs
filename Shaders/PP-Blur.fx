/* BEGIN GLOBAL CONSTANT BUFFER */
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

	output.Position = float4(input.Position.xyz,1);
	output.UV = input.UV;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 color = tex2D(InputMapSampler, float2(input.UV.x + 0.0025, input.UV.y + 0.0025));
	color += tex2D(InputMapSampler, float2(input.UV.x - 0.0025, input.UV.y - 0.0025));
	color += tex2D(InputMapSampler, float2(input.UV.x + 0.0025, input.UV.y - 0.0025));
	color += tex2D(InputMapSampler, float2(input.UV.x - 0.0025, input.UV.y + 0.0025));

	color /= 4.0f;

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