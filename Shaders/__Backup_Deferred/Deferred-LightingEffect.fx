/* BEGIN GLOBAL CONSTANT BUFFER */
// World
float4x4 WorldViewProjection;
float4x4 InverseViewProjection;

// Eye
float3 CameraPosition;

// General light parameters
float4x4 LightInverseView;
float3 LightColor;
float3 LightPosition;
float3 LightDirection;
float LightAttenuation;
float LightFalloff;

// Spot light parameters
float InnerConeAngle;
float OuterConeAngle;

// General shadow map parameters
float2 ShadowMapPixelSize;
float2 ShadowMapSize;
float ShadowBias;
bool SoftShadowEnabled;

// Spot and Point shadow map parameters
float4x4 LightViewProjection;

// Directional / Cascade shadow map parameters
float3 FrustumCorners[4];
float4x4 LightViewProjections[3];
float2 ClipPlanes[3];
float3 CascadeDistances;
/* END GLOBAL CONSTANT BUFFER */

/* BEGIN RESOURCE AND SAMPLER */
texture2D DepthMap;
sampler2D DepthMapSampler
{
	Texture = DepthMap;
	Filter = Point;
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Wrap;
};
texture2D NormalMap;
sampler2D NormalMapSampler
{
	Texture = NormalMap;
	Filter = Point;
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Wrap;
};
texture2D CookieMap;
sampler2D CookieMapSampler
{
	Texture = CookieMap;
	Filter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
	AddressW = Clamp;
};
texture2D ShadowMap;
sampler2D ShadowMapSampler
{
	Texture = ShadowMap;
	Filter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
	AddressW = Clamp;
};
texture2D ShadowMap2;
sampler2D ShadowMap2Sampler
{
	Texture = ShadowMap2;
	Filter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
	AddressW = Clamp;
};
/* END RESOURCE AND SAMPLER */

/* BEGIN INCLUDE */
// Calculate the size of one half of a pixel, to convert
// between texels and pixels from map size
float2 HalfPixel;

// Calculate the 2D screen position of a 3D position
float2 postProjToScreen(float4 position)
{
	float2 screenPosition = position.xy / position.w;
	return 0.5f * (float2(screenPosition.x, -screenPosition.y) + 1.0f);
}

float3 getFrustumRay(float2 texCoord)
{
	float frustum = texCoord.x + (texCoord.y * 2.0f);

	int index = 0;
	if (frustum > 2.5f)
	{
		index = 3;
	}
	else if (frustum > 1.5f)
	{
		index = 2;
	}
	else if (frustum > 0.5f)
	{
		index = 1;
	}

	return FrustumCorners[index];
}
/* END INCLUDE */

/* BEGIN FUNCTION */
// Shadow computation helper
float sampleShadowMap(float2 UV)
{
	if (UV.x < 0.0f || UV.x > 1.0f || UV.y < 0.0f || UV.y > 1.0f)
	{
		return 1.0f;
	}

	return tex2D(ShadowMapSampler, UV).r;
}

float sampleShadowMapPCF(float2 shadowTexCoord, float realDepth, int filterSize)
{
	float shadow = 0.0f;  
		
	float fRadius = (filterSize - 1) / 2.0f;
	float fWeightAccum = 0.0f;

	for (float y = -fRadius; y <= fRadius; y++)
	{
		for (float x = -fRadius; x <= fRadius; x++)
		{
			float2 vOffset = float2(x, y);				
			vOffset /= ShadowMapSize;
			float2 vSamplePoint = shadowTexCoord + vOffset;			
			float fDepth = tex2D(ShadowMapSampler, vSamplePoint).x;
			float fSample = 0.0f;

			if (realDepth <= fDepth + ShadowBias)
			{
				fSample = 1.0f;
			}
			
			// Edge tap smoothing
			float xWeight = 1.0f;
			float yWeight = 1.0f;
			
			if (x == -fRadius)
			{
				xWeight = 1.0f - frac(shadowTexCoord.x * ShadowMapSize.x);
			}
			else if (x == fRadius)
			{
				xWeight = frac(shadowTexCoord.x * ShadowMapSize.x);
			}
				
			if (y == -fRadius)
			{
				yWeight = 1.0f - frac(shadowTexCoord.y * ShadowMapSize.y);
			}
			else if (y == fRadius)
			{
				yWeight = frac(shadowTexCoord.y * ShadowMapSize.y);
			}
				
			shadow += fSample * xWeight * yWeight;
			fWeightAccum = xWeight * yWeight;
		}											
	}		
	
	shadow /= (filterSize * filterSize);
	shadow *= 1.55f;
	
	return shadow;
}

float sampleShadowMapPCF4(float2 shadowTexCoord, float realDepth)
{
	realDepth = realDepth - ShadowBias;

	// Get the current depth stored in the shadow map
	float samples[4];	
	float r0 = tex2D(ShadowMapSampler, shadowTexCoord).r;
	if (r0 < realDepth)
	{
		samples[0] = 0.0f;
	}
	else
	{
		samples[0] = 1.0f;
	}
	float r1 = tex2D(ShadowMapSampler, shadowTexCoord + float2(0.0f, 2.0f) * ShadowMapPixelSize).r;
	if (r1 < realDepth)
	{
		samples[1] = 0.0f;
	}
	else
	{
		samples[1] = 1.0f;
	}
	float r2 = tex2D(ShadowMapSampler, shadowTexCoord + float2(2.0f, 0.0f) * ShadowMapPixelSize).r;
	if (r2 < realDepth)
	{
		samples[2] = 0.0f;
	}
	else
	{
		samples[2] = 1.0f;
	}
	float r3 = tex2D(ShadowMapSampler, shadowTexCoord + float2(2.0f, 2.0f) * ShadowMapPixelSize).r;
	if (r3 < realDepth)
	{
		samples[3] = 0.0f;
	}
	else
	{
		samples[3] = 1.0f;
	}	
    
	// Determine the lerp amounts           
	float2 lerps = frac(shadowTexCoord * ShadowMapSize);
	// lerp between the shadow values to calculate our light amount
	float shadow = lerp(lerp(samples[0], samples[1], lerps.y), lerp( samples[2], samples[3], lerps.y), lerps.x);							  
				
	return shadow;
}

float sampleShadowMap2(float2 UV)
{
	if (UV.x < 0.0f || UV.x > 1.0f || UV.y < 0.0f || UV.y > 1.0f)
	{
		return 1.0f;
	}

	return tex2D(ShadowMap2Sampler, UV).r;
}

float sampleShadowMap2PCF(float2 shadowTexCoord, float realDepth, int filterSize)
{
	float shadow = 0.0f;  
		
	float fRadius = (filterSize - 1) / 2.0f;
	float fWeightAccum = 0.0f;
	
	for (float y = -fRadius; y <= fRadius; y++)
	{
		for (float x = -fRadius; x <= fRadius; x++)
		{
			float2 vOffset = float2(x, y);				
			vOffset /= ShadowMapSize;
			float2 vSamplePoint = shadowTexCoord + vOffset;			
			float fDepth = tex2D(ShadowMap2Sampler, vSamplePoint).x;
			float fSample = 0.0f;

			if (realDepth <= fDepth + ShadowBias)
			{
				fSample = 1.0f;
			}
			
			// Edge tap smoothing
			float xWeight = 1.0f;
			float yWeight = 1.0f;
			
			if (x == -fRadius)
			{
				xWeight = 1.0f - frac(shadowTexCoord.x * ShadowMapSize.x);
			}
			else if (x == fRadius)
			{
				xWeight = frac(shadowTexCoord.x * ShadowMapSize.x);
			}
				
			if (y == -fRadius)
			{
				yWeight = 1.0f - frac(shadowTexCoord.y * ShadowMapSize.y);
			}
			else if (y == fRadius)
			{
				yWeight = frac(shadowTexCoord.y * ShadowMapSize.y);
			}
				
			shadow += fSample * xWeight * yWeight;
			fWeightAccum = xWeight * yWeight;
		}											
	}		
	
	shadow /= (filterSize * filterSize);
	shadow *= 1.55f;
	
	return shadow;
}

float sampleShadowMap2PCF4(float2 shadowTexCoord, float realDepth)
{
	realDepth = realDepth - ShadowBias;

	// Get the current depth stored in the shadow map
	float samples[4];	
	float r0 = tex2D(ShadowMap2Sampler, shadowTexCoord).r;
	if (r0 < realDepth)
	{
		samples[0] = 0.0f;
	}
	else
	{
		samples[0] = 1.0f;
	}
	float r1 = tex2D(ShadowMap2Sampler, shadowTexCoord + float2(0.0f, 2.0f) * ShadowMapPixelSize).r;
	if (r1 < realDepth)
	{
		samples[1] = 0.0f;
	}
	else
	{
		samples[1] = 1.0f;
	}
	float r2 = tex2D(ShadowMap2Sampler, shadowTexCoord + float2(2.0f, 0.0f) * ShadowMapPixelSize).r;
	if (r2 < realDepth)
	{
		samples[2] = 0.0f;
	}
	else
	{
		samples[2] = 1.0f;
	}
	float r3 = tex2D(ShadowMap2Sampler, shadowTexCoord + float2(2.0f, 2.0f) * ShadowMapPixelSize).r;
	if (r3 < realDepth)
	{
		samples[3] = 0.0f;
	}
	else
	{
		samples[3] = 1.0f;
	}	
    
	// Determine the lerp amounts           
	float2 lerps = frac(shadowTexCoord * ShadowMapSize);
	// lerp between the shadow values to calculate our light amount
	float shadow = lerp(lerp(samples[0], samples[1], lerps.y), lerp( samples[2], samples[3], lerps.y), lerps.x);							  
				
	return shadow;
}
/* END FUNCTION */

/* BEGIN SEMANTIC */
struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 LightPosition : TEXCOORD0;
};

struct Directional_VertexShaderInput
{
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

struct Directional_VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

struct DirectionalWithShadow_VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 FrustumRay : TEXCOORD1;
};
/* END SEMANTIC */

/* BEGIN VERTEX AND PIXEL SHADER */
VertexShaderOutput Point_VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = mul(input.Position, WorldViewProjection);
	output.LightPosition = output.Position;

    return output;
}

float4 Point_PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// Find the pixel coordinates of the input position in the depth
	// and normal textures
	float2 texCoord = postProjToScreen(input.LightPosition) + HalfPixel;

	// Extract the depth for this pixel from the depth map
	float4 depth = tex2D(DepthMapSampler, texCoord);

	// Recreate the position with the UV coordinates and depth value
	float4 position;
	position.x = texCoord.x * 2.0f - 1.0f;
	position.y = (1.0f - texCoord.y) * 2.0f - 1.0f;
	position.z = depth.r;
	position.w = 1.0f;

	// Transform position from screen space to world space
	position = mul(position, InverseViewProjection);
	position.xyz /= position.w;
	position.w = 1.0f;

	// Get normal map
	float4 normalMap = tex2D(NormalMapSampler, texCoord);

	// Extract the normal from the normal map and move from
	// 0 to 1 range to -1 to 1 range
	float4 normal = (normalMap - 0.5f) * 2.0f;

	// Perform the lighting calculations for a point light
	float3 lightDirection = normalize(LightPosition - position.xyz);

	// Compute diffuse light
	float lighting = clamp(dot(normal.xyz, lightDirection), 0.0f, 1.0f);

	// Attenuate the light to simulate a point light
	float d = distance(LightPosition, position.xyz);
	float att = 1.0f - pow(clamp(d / LightAttenuation, 0.0f, 1.0f), LightFalloff);

	// Compute specular light
	float3 reflection = normalize(reflect(lightDirection, normal.xyz));
    float3 view = normalize(CameraPosition - position.xyz);
    float specular = pow(saturate(dot(reflection, view)), normalMap.a * 255.0f);

    return float4(LightColor * lighting * att, specular);
}

float4 PointWithShadow_PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// Find the pixel coordinates of the input position in the depth
	// and normal textures
	float2 texCoord = postProjToScreen(input.LightPosition) + HalfPixel;

	// Extract the depth for this pixel from the depth map
	float4 depth = tex2D(DepthMapSampler, texCoord);

	// Recreate the position with the UV coordinates and depth value
	float4 position;
	position.x = texCoord.x * 2.0f - 1.0f;
	position.y = (1.0f - texCoord.y) * 2.0f - 1.0f;
	position.z = depth.r;
	position.w = 1.0f;

	// Transform position from screen space to world space
	position = mul(position, InverseViewProjection);
	position.xyz /= position.w;
	position.w = 1.0f;

	// Get normal map
	float4 normalMap = tex2D(NormalMapSampler, texCoord);

	// Extract the normal from the normal map and move from
	// 0 to 1 range to -1 to 1 range
	float4 normal = (normalMap - 0.5f) * 2.0f;

	// Perform the lighting calculations for a point light
	float3 lightDirection = normalize(LightPosition - position.xyz);

	// Compute diffuse light
	float lighting = clamp(dot(normal.xyz, lightDirection), 0.0f, 1.0f);

	// Attenuate the light to simulate a point light
	float d = distance(LightPosition, position.xyz);
	float att = 1.0f - pow(clamp(d / LightAttenuation, 0.0f, 1.0f), LightFalloff);

	// Compute shadow
	// Calculate homogenous position with respect to light
	float4 lightScreenPosition = mul(position, LightViewProjection);

	float lightScreenPositionLength = length(lightScreenPosition);
    lightScreenPosition /= lightScreenPositionLength;

	// Get real depth
	float realDepth = (lightScreenPosition.z / lightScreenPosition.w);

	// Get depth / shadow of position from light view
	float shadow = 1.0f;
	// Begin calculate shadow
	if(lightScreenPosition.z >= 0.0f)
    {
        float2 shadowTextCoordFront;
        shadowTextCoordFront.x =  (lightScreenPosition.x /  (1.0f + lightScreenPosition.z)) * 0.5f + 0.5f; 
        shadowTextCoordFront.y =  1.0f - ((lightScreenPosition.y /  (1.0f + lightScreenPosition.z)) * 0.5f + 0.5f); 	
    
		if(SoftShadowEnabled)
		{
			shadow = sampleShadowMapPCF(shadowTextCoordFront, realDepth, 7);
		}  
		else
		{
			realDepth -= ShadowBias;
			shadow = sampleShadowMapPCF4(shadowTextCoordFront, realDepth);
		}
    }
    else
    {
        // for the back the z has to be inverted		
        float2 shadowTextCoordBack;
        shadowTextCoordBack.x =  (lightScreenPosition.x /  (1.0f - lightScreenPosition.z)) * 0.5f + 0.5f; 
        shadowTextCoordBack.y =  1.0f - ((lightScreenPosition.y /  (1.0f - lightScreenPosition.z)) * 0.5f + 0.5f); 
        
		if(SoftShadowEnabled)
		{
			shadow = sampleShadowMap2PCF(shadowTextCoordBack, realDepth, 7);
		}  
		else
		{
			realDepth -= ShadowBias;
			shadow = sampleShadowMap2PCF4(shadowTextCoordBack, realDepth);
		}
    }

	// Compute specular light
	float3 reflection = normalize(reflect(lightDirection, normal.xyz));
    float3 view = normalize(CameraPosition - position.xyz);
    float specular = pow(saturate(dot(reflection, view)), normalMap.a * 255.0f);

    return float4(LightColor * lighting * att * shadow, specular);
}

float4 Spot_PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// Find the pixel coordinates of the input position in the depth
	// and normal textures
	float2 texCoord = postProjToScreen(input.LightPosition) + HalfPixel;

	// Extract the depth for this pixel from the depth map
	float4 depth = tex2D(DepthMapSampler, texCoord);

	// Recreate the position with the UV coordinates and depth value
	float4 position;
	position.x = texCoord.x * 2.0f - 1.0f;
	position.y = (1.0f - texCoord.y) * 2.0f - 1.0f;
	position.z = depth.r;
	position.w = 1.0f;

	// Transform position from screen space to world space
	position = mul(position, InverseViewProjection);
	position.xyz /= position.w;
	position.w = 1.0f;

	// Get normal map
	float4 normalMap = tex2D(NormalMapSampler, texCoord);

	// Extract or decode the normal from the normal map and move from
	// 0 to 1 range to -1 to 1 range
	float4 normal = (normalMap - 0.5f) * 2.0f;

	// Perform the lighting calculations for a spot light
	float3 lightDirection = LightPosition - position.xyz;
	float att = 1.0f - length(lightDirection) / LightAttenuation;
	lightDirection = normalize(lightDirection);

	// Compute diffuse light
	float lighting = max(0.0f, dot(normal.xyz, lightDirection));

	// Compute spot attenuation
	float2 cosAngles = cos(float2(OuterConeAngle, InnerConeAngle) * 0.5f);
	float d = dot(-lightDirection, normalize(LightDirection));
	float spotAtten = smoothstep(cosAngles[0], cosAngles[1], d);
	att *= spotAtten * LightFalloff;	

	// Compute specular light
	float3 reflection = normalize(reflect(lightDirection, normal.xyz));
    float3 view = normalize(CameraPosition - position.xyz);
    float specular = pow(saturate(dot(reflection, view)), normalMap.a * 255.0f);

    return float4(LightColor * lighting * att, specular);
}

float4 SpotWithShadow_PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// Find the pixel coordinates of the input position in the depth
	// and normal textures
	float2 texCoord = postProjToScreen(input.LightPosition) + HalfPixel;

	// Extract the depth for this pixel from the depth map
	float4 depth = tex2D(DepthMapSampler, texCoord);

	// Recreate the position with the UV coordinates and depth value
	float4 position;
	position.x = texCoord.x * 2.0f - 1.0f;
	position.y = (1.0f - texCoord.y) * 2.0f - 1.0f;
	position.z = depth.r;
	position.w = 1.0f;

	// Transform position from screen space to world space
	position = mul(position, InverseViewProjection);
	position.xyz /= position.w;
	position.w = 1.0f;

	// Get normal map
	float4 normalMap = tex2D(NormalMapSampler, texCoord);

	// Extract the normal from the normal map and move from
	// 0 to 1 range to -1 to 1 range
	float4 normal = (normalMap - 0.5f) * 2.0f;

	// Perform the lighting calculations for a spot light
	float3 lightDirection = LightPosition - position.xyz;
	float att = 1.0f - length(lightDirection) / LightAttenuation;
	lightDirection = normalize(lightDirection);

	// Compute diffuse light
	float lighting = max(0.0f, dot(normal.xyz, lightDirection));

	// Compute spot attenuation
	float2 cosAngles = cos(float2(OuterConeAngle, InnerConeAngle) * 0.5f);
	float d = dot(-lightDirection, normalize(LightDirection));
	float spotAtten = smoothstep(cosAngles[0], cosAngles[1], d);
	att *= spotAtten * LightFalloff;

	// Calculate homogenous position with respect to light
	float4 lightScreenPosition = mul(position, LightViewProjection);

	// Find sample position in shadow map
	float2 shadowTexCoord = postProjToScreen(lightScreenPosition) + ShadowMapPixelSize;

	// Get real depth
	float realDepth = (lightScreenPosition.z / lightScreenPosition.w);

	// Get depth / shadow of position from light view
	float shadow = 1.0f;
	// Begin calculate shadow
	if (SoftShadowEnabled)
	{
		shadow = sampleShadowMapPCF(shadowTexCoord, realDepth, 7);
	}
	else
	{
		realDepth -= ShadowBias;
		shadow = sampleShadowMapPCF4(shadowTexCoord, realDepth);
	}
	// End calculate shadow

	// Compute specular light
	float3 reflection = normalize(reflect(lightDirection, normal.xyz));
    float3 view = normalize(CameraPosition - position.xyz);
    float specular = pow(saturate(dot(reflection, view)), normalMap.a * 255.0f);

    return float4(LightColor * lighting * att * shadow, specular);
}

Directional_VertexShaderOutput Directional_VertexShaderFunction(Directional_VertexShaderInput input)
{
    Directional_VertexShaderOutput output = (Directional_VertexShaderOutput)0;

    output.Position = mul(input.Position, 1.0f);
	output.UV = input.UV;

    return output;
}

DirectionalWithShadow_VertexShaderOutput DirectionalWithShadow_VertexShaderFunction(Directional_VertexShaderInput input)
{
    DirectionalWithShadow_VertexShaderOutput output = (DirectionalWithShadow_VertexShaderOutput)0;

    output.Position = mul(input.Position, 1.0f);
	output.UV = input.UV;
	output.FrustumRay = getFrustumRay(input.UV);

    return output;
}

float4 Directional_PixelShaderFunction(Directional_VertexShaderOutput input) : COLOR0
{
	float2 texCoord = input.UV + HalfPixel;
	
	// Extract the depth for this pixel from the depth map
	float4 depth = tex2D(DepthMapSampler, texCoord);

	// Recreate the position with the UV coordinates and depth value
	float4 position;
	position.x = texCoord.x * 2.0f - 1.0f;
	position.y = (1.0f - texCoord.y) * 2.0f - 1.0f;
	position.z = depth.r;
	position.w = 1.0f;

	// Transform position from screen space to world space
	position = mul(position, InverseViewProjection);
	position.xyz /= position.w;
	position.w = 1.0f;

	// Get normal map
	float4 normalMap = tex2D(NormalMapSampler, texCoord);

	// Extract the normal from the normal map and move from
	// 0 to 1 range to -1 to 1 range
	float4 normal = (normalMap - 0.5f) * 2.0f;

	// Perform lighting for directional light
    float3 lightDirection = normalize(LightDirection);

    // Compute diffuse light
    float lighting = saturate(dot(-lightDirection, normal.xyz));

	// Compute specular light
	float3 reflection = normalize(reflect(lightDirection, normal.xyz));
    float3 view = normalize(CameraPosition - position.xyz);
    float specular = pow(saturate(dot(reflection, view)), normalMap.a * 255.0f);

	return float4(LightColor * lighting, specular);
}

float4 DirectionalWithShadow_PixelShaderFunction(DirectionalWithShadow_VertexShaderOutput input) : COLOR0
{
	float2 texCoord = input.UV + HalfPixel;
	
	// Extract the depth for this pixel from the depth map
	float4 depth = tex2D(DepthMapSampler, texCoord);

	// Recreate the position with the UV coordinates and depth value
	float4 position;
	position.x = texCoord.x * 2.0f - 1.0f;
	position.y = (1.0f - texCoord.y) * 2.0f - 1.0f;
	position.z = depth.r;
	position.w = 1.0f;

	// Transform position from screen space to world space
	position = mul(position, InverseViewProjection);
	position.xyz /= position.w;
	position.w = 1.0f;

	// Get normal map
	float4 normalMap = tex2D(NormalMapSampler, texCoord);

	// Extract the normal from the normal map and move from
	// 0 to 1 range to -1 to 1 range
	float4 normal = (normalMap - 0.5f) * 2.0f;

	// Perform lighting for directional light
    float3 lightDirection = normalize(LightDirection);

    // Compute diffuse light
    float lighting = saturate(dot(-lightDirection, normal.xyz));

	// Compute shadow
	float3 positionCopy = input.FrustumRay * depth.r;
	// Figure out which split this pixel belongs to, based on view-space depth.
	float3 weights = float3(0.0f, 0.0f, 0.0f);
	if (positionCopy.z < CascadeDistances.x)
	{
		weights.x = 0.0f;
	}
	else
	{
		weights.x = 1.0f;
	}
	if (positionCopy.z < CascadeDistances.y)
	{
		weights.y = 0.0f;
	}
	else
	{
		weights.y = 1.0f;
	}
	if (positionCopy.z < CascadeDistances.z)
	{
		weights.z = 0.0f;
	}
	else
	{
		weights.z = 1.0f;
	}
	weights.xy -= weights.yz;

	float4x4 lightViewProjection = LightViewProjections[0] * weights.x + LightViewProjections[1] * weights.y + LightViewProjections[2] * weights.z;		

	// Remember that we need to find the correct cascade into our cascade atlas
	float fOffset = weights.y * 0.33333f + weights.z * 0.666666f;

	// Find the position of this pixel in light space
	float4 lightScreenPosition = mul(position, lightViewProjection);
    
	// Find sample position in shadow map
	float2 shadowTexCoord = postProjToScreen(lightScreenPosition) + ShadowMapPixelSize;	
	shadowTexCoord.x = shadowTexCoord.x * 0.3333333f + fOffset;

	// Calculate the current pixel depth
	// The bias is used to prevent floating point errors that occur when
	// the pixel of the occluder is being drawn
	float realDepth = (lightScreenPosition.z / lightScreenPosition.w);

	// The pixel can be outside of shadow distance, so skip it in that case
	float shadowSkip = 0.0f;
	if (ClipPlanes[2].y > position.z)
	{
		shadowSkip = 1.0f;
	}

	// Get depth / shadow of position from light view
	float shadow = 1.0f;
	if(SoftShadowEnabled)
	{
		shadow = shadowSkip + sampleShadowMapPCF(shadowTexCoord, realDepth, 7) * (1.0f - shadowSkip);
	}
	else
	{
		realDepth -= ShadowBias;
		shadow = shadowSkip + sampleShadowMapPCF4(shadowTexCoord, realDepth) * (1.0f - shadowSkip);
	}

	// Compute specular light
	float3 reflection = normalize(reflect(lightDirection, normal.xyz));
    float3 view = normalize(CameraPosition - position.xyz);
    float specular = pow(saturate(dot(reflection, view)), normalMap.a * 255.0f);

	return float4(LightColor * lighting * shadow, specular);
}
/* END VERTEX AND PIXEL SHADER */

/* BEGIN TECHNIQUE */
technique PointTechnique
{
    pass Pass0
    {
		SetVertexShader(Point_VertexShaderFunction);
		SetPixelShader(Point_PixelShaderFunction);
    }
}

/*technique PointWithShadowTechnique
{
    pass Pass0
    {
		SetVertexShader(Point_VertexShaderFunction);
		SetPixelShader(PointWithShadow_PixelShaderFunction);
    }
}*/

technique SpotTechnique
{
    pass Pass0
    {
		SetVertexShader(Point_VertexShaderFunction);
		SetPixelShader(Spot_PixelShaderFunction);
    }
}

technique SpotWithShadowTechnique
{
    pass Pass0
    {
		SetVertexShader(Point_VertexShaderFunction);
		SetPixelShader(SpotWithShadow_PixelShaderFunction);
    }
}

technique DirectionalTechnique
{
    pass Pass0
    {
		SetVertexShader(Directional_VertexShaderFunction);
		SetPixelShader(Directional_PixelShaderFunction);
    }
}

technique DirectionalWithShadowTechnique
{
    pass Pass0
    {
		SetVertexShader(DirectionalWithShadow_VertexShaderFunction);
		SetPixelShader(DirectionalWithShadow_PixelShaderFunction);
    }
}
/* END TECHNIQUE */

