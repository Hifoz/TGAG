#ifndef __TEXTURES_HLSL__
#define __TEXTURES_HLSL__

#include "utils.hlsl"

/*
	WIP!
	Porting TerrainTextureGenerator from C# to HLSL.
	Reason wht doing texture generation on GPU is better:
	1. More variety. No more choosing between pre-generated textures, textures are now generated per texel in the world.
	2. Better distribution of resources. The CPU is already busy with terrain generation and many other things, and the GPU is doing a relatively small amount of work.
		2.1 Performance hit on GPU seems pretty small from a fairly quick test done with grass being done on the GPU.
*/


// Generate texture for grass
// pos: worldposition to sample
float4 grassTex(float3 pos) {
	float hue = 0.2;
	float saturation = 0.85;
	float value = 0.6;


	float f = 2;
	float v = noise(pos, f) * 0.1 +
		noise(pos, f * 3) * 0.1 +
		noise(pos, f * 6) * 0.1 +
		noise(pos, f * 10) * 0.1 +
		noise(pos, f * 30) * 0.15 +
		noise(pos, f * 70) * 0.15 +
		noise(pos, f * 100) * 0.15;

	value += v;
	value = clamp(value * 0.5, 0, 1);

	float4 o = float4(HSVtoRGB(float3(hue, saturation, value)), 1);

	return o;
}

// Generate grass for the side of a block
// samplePos: worldposition to sample
// pos: worldposition of fragment
// sampleDistance: distance between fragment position and sample positions
float4 grassSideTex(float3 samplePos, float3 pos, float sampleDistance) {
	float blockSamplePosY = (samplePos.y - 0.5) % 1;
	float blockPosY = (pos.y - 0.5) % 1;

	if (blockSamplePosY < blockPosY)
		blockSamplePosY -= (samplePos.y - pos.y * 2);

	float grassHeight = 0.85 - 
		sampleDistance - 
		noise(samplePos, 10) * 0.1;


	if(blockPosY > grassHeight)
		return grassTex(samplePos);
	return float4(0, 0, 0, 0);
}

// Generate texture for dirt
// pos: worldposition to sample
float4 dirtTex(float3 pos) {
	float hue = 0.083;
	float saturation = 0.6;
	float value = 0.6;

	float3 modPos = float3(
		pos.x + noise(pos),
		pos.y + noise(pos),
		pos.z + noise(pos)
		);
	float3 seed = float3(552, 1556, 663);
	float3 modPos2 = float3(
		pos.x + noise(pos + seed),
		pos.y + noise(pos + seed),
		pos.z + noise(pos + seed)
		);


	float f = 4;
	float v = noise(modPos, f * 0.1) * 0.15 +
		noise(modPos, f * 0.7) * 0.15 +
		noise(modPos + modPos2, f * 0.3) * 0.1 +
		noise(modPos2, f) * 0.1 +
		noise(modPos, f * 2) * 0.05;

	value = (value + v * 0.6) * 0.7;

	return float4(HSVtoRGB(float3(hue, saturation, value)), 1);
}

// Generate texture for sand
// pos: worldposition to sample
float4 sandTex(float3 pos) {
	float hue = 0.13;
	float saturation = 0.5;
	float value = RGBtoHSV(dirtTex(pos))[2] + 0.15;
	return float4(HSVtoRGB(float3(hue, saturation, value)), 1);
}

// Generate texture for snow
// pos: worldposition to sample
float4 snowTex(float3 pos) {
	float hue = 0.55;
	float saturation = 0.05;
	float value = RGBtoHSV(dirtTex(pos * 0.5))[2] * 0.7 + 0.4;
	return float4(HSVtoRGB(float3(hue, saturation, value)), 1);
}

// Generate grass for the side of a block
// samplePos: worldposition to sample
// pos: worldposition of fragment
// sampleDistance: distance between fragment position and sample positions
float4 snowSideTex(float3 samplePos, float3 pos, float sampleDistance) {
	float blockSamplePosY = (samplePos.y - 0.5) % 1;
	float blockPosY = (pos.y - 0.5) % 1;

	if (blockSamplePosY < blockPosY)
		blockSamplePosY -= (samplePos.y - pos.y * 2);

	float snowHeight = 0.85 - 
		sampleDistance - 
		noise(samplePos, 10) * 0.1;

	if (blockPosY > snowHeight)
		return snowTex(samplePos);
	return float4(0, 0, 0, 0);
}

// Generate texture for stone
// pos: worldposition to sample
float4 stoneTex(float3 pos) {
	// TODO create this

	return float4(0.5, 0.5, 0.5, 1);
}

// Generate texture for wood
// pos: worldposition to sample
float4 woodTex(float3 pos) {
	// WIP

	float hue = 0.083;
	float saturation = 0.5;
	float value = 0.8;

	float freq = 4;
	pos.xz *= 7;
	pos.y *= 0.7;
	float v = noise(pos, freq) *
		noise(pos + 5124, freq - 2) *
		noise(pos + 661, freq + 1) *
		0.3 + 0.1;

	float v2 = noise(pos + 4115, freq) *
		noise(pos + 2125, freq - 2) *
		noise(pos + 6615, freq + 1) *
		0.3 + 0.1;
	value = clamp(0.4 - v * v2 * 4, 0, 1);


	return float4(HSVtoRGB(float3(hue, saturation, value)), 1);
}

// Generate texture for leaves
// pos: worldposition to sample
float4 leafTex(float3 pos) {
	float hue = 0.2;
	float saturation = 0.85;
	float value = 0.2;

	float f = 4;
	float v = noise(pos, f) * 0.1 +
		noise(pos, f * 3) * 0.1 +
		noise(pos, f * 6) * 0.1 +
		noise(pos, f * 10) * 0.1 +
		noise(pos, f * 30) * 0.13 +
		noise(pos, f * 70) * 0.1 +
		noise(pos, f * 100) * 0.05 +
		noise(pos, f * 1.5) * noise(pos, f * 2) * 0.5;

	value += v;
	value = clamp(value, 0, 1);

	return float4(HSVtoRGB(float3(hue, saturation, value)), 1);
}

// Generate texture for water
// pos: worldposition to sample
float4  waterTex(float3 pos) {
	// TODO more than simple color
	return float4(0.4, 0.4, 1, 0.95);
}

// Sample the texel value for a slice at samplePos
// slice: type of the block
// pos: worldposition of fragment
// samplePos: worldposition to sample
// sampleDistance: distance between fragment position and sample positions
float4 sampleTexelValue(int slice, float3 pos, float3 samplePos, int sampleDistance) {
	switch (slice + 1) {
	case 1: // Dirt
		return dirtTex(samplePos);
	case 2: // Stone
		return stoneTex(samplePos);
	case 3: // Sand
		return sandTex(samplePos);
	case 4: // Grass top
		return grassTex(samplePos);
	case 5: // Grass side
		return grassSideTex(samplePos, pos, sampleDistance);
	case 6: // Snow top
		return snowTex(samplePos);
	case 7: // Snow side
		return snowSideTex(samplePos, pos, sampleDistance);
	case 8: // Wood
		return woodTex(samplePos);
	case 9: // Leaf
		return leafTex(samplePos);
	case 10: // Water
		return waterTex(samplePos);
	default:
		return float4(1, 1, 1, 0);
	}
}


// Gets the value for a texel on a face 
// slice: type of the block
// modSlice: type of the block modifier
// pos: worldposition of fragment
// posEye: position of fragment in relation to camera
float4 getTexel(int slice, int modSlice, float3 pos, float3 posEye) {
	float distFromEye = length(posEye);
	
	float textureSize = 512;
	pos = ((int3)(pos * textureSize)) / textureSize;

	float sampleDistance = 1;

	if (distFromEye > 110) {
		sampleDistance = 128;
	} else if(distFromEye > 60) {
		sampleDistance = 48;
	} else if(distFromEye > 40) {
		sampleDistance = 16;
	} else if(distFromEye > 20) {
		sampleDistance = 8;
	} else if(distFromEye > 10) {
		sampleDistance = 4;
	} else if (distFromEye > 5) {
		sampleDistance = 2;
	}
	sampleDistance /= textureSize;

	float4 texelTotal = float4(0, 0, 0, 0);
	float samples = 0;
	for (int x = 0; x < 2; x++) {
		for (int y = 0; y < 2; y++) {
			for (int z = 0; z < 2; z++) {
				float4 baseVal = sampleTexelValue(slice, pos, pos + float3(x * sampleDistance, y * sampleDistance, z * sampleDistance), sampleDistance);
				if (modSlice == 0) {
					texelTotal += baseVal;
				}
				else {
					float4 modVal = sampleTexelValue(modSlice, pos, pos + float3(x * sampleDistance, y * sampleDistance, z * sampleDistance), sampleDistance);
					float4 val;
					texelTotal.rgb += float4(HSVtoRGB(lerp(RGBtoHSV(modVal.rgb), RGBtoHSV(baseVal.rgb), 1 - modVal.a)), 1);
					//texelTotal.rgb += lerp(modVal, baseVal, 1 - modVal.a).rgb;
					texelTotal.a += min(modVal.a + baseVal.a, 1);
				}
				samples++;
			}
		}
	}
	

	return texelTotal / samples;
}


#endif // __TEXTURES_HLSL__