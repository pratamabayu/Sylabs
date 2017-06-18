/*
Written by Pratama Bayu Widagdo
Email : nobley.1928@hotmail.com
*/

// Constants
float4x4 Transform;

// Resources
texture2D Texture;

// Samplers
sampler2D TextureSampler
{
	Texture = Texture;
	Filter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
	AddressW = Clamp;
};

// Semantics
struct VS_IN
{
	float4 pos : POSITION0;
	float4 col : COLOR0;
	float2 tex : TEXCOORD0;
};

struct PS_IN
{
	float4 pos : POSITION0;
	float4 col : COLOR0;
	float2 tex : TEXCOORD0;
};

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;
	
	output.pos = mul(input.pos, Transform);
	output.col = input.col;
	output.tex = input.tex;

	return output;
}

float4 PS(PS_IN input) : COLOR0
{
	return tex2D(TextureSampler, input.tex) * input.col;
}

technique MainTechnique
{
	pass Pass0
	{
		SetVertexShader(VS);
		SetPixelShader(PS);
	}
}