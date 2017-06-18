/*
Written by Pratama Bayu W
Email : nobley.1928@hotmail.com
*/

// Constants
float4x4 World;
float4x4 View;
float4x4 Projection;

// Semantics
struct VS_IN
{
	float4 pos : POSITION0;
	float4 col : COLOR0;
};

struct PS_IN
{
	float4 pos : POSITION0;
	float4 col : COLOR0;
};

PS_IN VS( VS_IN input )
{
	PS_IN output = (PS_IN)0;
	
	float4 worldPosition = mul(input.pos, World);
	float4 viewPosition = mul(worldPosition, View);
	output.pos = mul(viewPosition, Projection);

	output.col = input.col;
	
	return output;
}

float4 PS( PS_IN input ) : COLOR0
{
	return input.col;
}

technique MainTechnique
{
	pass pass0
	{
		SetVertexShader(VS);
		SetPixelShader(PS);
	}
}