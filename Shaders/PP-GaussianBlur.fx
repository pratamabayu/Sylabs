/* BEGIN GLOBAL CONSTANT BUFFER */
float2 Offsets0;
float2 Offsets1;
float2 Offsets2;
float2 Offsets3;
float2 Offsets4;
float2 Offsets5;
float2 Offsets6;
float2 Offsets7;
float2 Offsets8;
float2 Offsets9;
float2 Offsets10;
float2 Offsets11;
float2 Offsets12;
float2 Offsets13;
float2 Offsets14;

float Weights0;
float Weights1;
float Weights2;
float Weights3;
float Weights4;
float Weights5;
float Weights6;
float Weights7;
float Weights8;
float Weights9;
float Weights10;
float Weights11;
float Weights12;
float Weights13;
float Weights14;
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
    float4 color = tex2D(InputMapSampler, input.UV + Offsets0) * Weights0;
	color += tex2D(InputMapSampler, input.UV + Offsets1) * Weights1;
	color += tex2D(InputMapSampler, input.UV + Offsets2) * Weights2;
	color += tex2D(InputMapSampler, input.UV + Offsets3) * Weights3;
	color += tex2D(InputMapSampler, input.UV + Offsets4) * Weights4;
	color += tex2D(InputMapSampler, input.UV + Offsets5) * Weights5;
	color += tex2D(InputMapSampler, input.UV + Offsets6) * Weights6;
	color += tex2D(InputMapSampler, input.UV + Offsets7) * Weights7;
	color += tex2D(InputMapSampler, input.UV + Offsets8) * Weights8;
	color += tex2D(InputMapSampler, input.UV + Offsets9) * Weights9;
	color += tex2D(InputMapSampler, input.UV + Offsets10) * Weights10;
	color += tex2D(InputMapSampler, input.UV + Offsets11) * Weights11;
	color += tex2D(InputMapSampler, input.UV + Offsets12) * Weights12;
	color += tex2D(InputMapSampler, input.UV + Offsets13) * Weights13;
	color += tex2D(InputMapSampler, input.UV + Offsets14) * Weights14;

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