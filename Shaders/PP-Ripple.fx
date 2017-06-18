/* BEGIN GLOBAL CONSTANT BUFFER */
float Wave;
float Distortion;
float2 Center;
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
	float2 distance = abs(input.UV - Center);
    float scalar = length(distance);

    // invert the scale so 1 is centerpoint
    scalar = abs(1.0f - scalar);
        
    // calculate how far to distort for this pixel    
    float sinoffset = sin(Wave / scalar);
    sinoffset = clamp(sinoffset, 0.0f, 1.0f);
    
    // calculate which direction to distort
    float sinsign = cos(Wave / scalar);    
    
    // reduce the distortion effect
    sinoffset = sinoffset * Distortion / 32.0f;
    
    // pick a pixel on the screen for this pixel, based on
    // the calculated offset and direction
    float4 color = tex2D(InputMapSampler, input.UV + (sinoffset * sinsign));    
            
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