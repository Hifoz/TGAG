#ifndef __TEXTURES_HLSL__
#define __TEXTURES_HLSL__

#include "utils.hlsl"

/*
	Experimental/WIP!
	Porting TerrainTextureGenerator from C# to HLSL.
	Reason wht doing texture generation on GPU is better:
	1. More variety. No more choosing between pre-generated textures, textures are now generated per texel in the world.
	2. Better distribution of resources. The CPU is already busy with terrain generation and many other things, and the GPU is doing a relatively small amount of work.
		2.1 Performance hit on GPU seems pretty small from a fairly quick test done with grass being done on the GPU.

	NB!  Does not currently work with tree textures!
*/


// Generate texture for grass
// pos: worldpos of fragment
// maxFreq: highest noise frequency we want to use
float4 grassTex(float3 pos, float maxFreq) {
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


	float noiseFreq[7] = {
		f,
		f * 3,
		f * 6,
		f * 10,
		f * 30,
		f * 70,
		f * 100
	};

	float noiseMag[7] = {
		0.1,
		0.1,
		0.1,
		0.1,
		0.15,
		0.15,
		0.15
	};

	for (int i = 0; i < 7; i++) {
		if (noiseFreq[i] <= maxFreq)
			value += noise(pos, noiseFreq[i]) * noiseMag[i];
		else
			value += noiseMag[i] * 0.5;
	}


	value = clamp(value * 0.5, 0, 1);

	float4 o = float4(HSVtoRGB(float3(hue, saturation, value)), 1);

	return o;
}

// Generate grass for the side of a block
float4 grassSideTex(float3 pos, float maxFreq) {
	float blockPosY = (pos.y - 0.5) % 1;

	float bGh = 0.85;

	float gH = bGh - noise(pos, 80) * 0.1;


	if(blockPosY > gH)
		return grassTex(pos, maxFreq);
	return float4(0, 0, 0, 0);
}

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

float4 sandTex(float3 pos) {
	float hue = 0.13;
	float saturation = 0.5;
	float value = RGBtoHSV(dirtTex(pos))[2] + 0.15;
	return float4(HSVtoRGB(float3(hue, saturation, value)), 1);
}

float4 snowTex(float3 pos) {
	float hue = 0.55;
	float saturation = 0.05;
	float value = RGBtoHSV(dirtTex(pos * 0.5))[2] * 0.7 + 0.4;
	return float4(HSVtoRGB(float3(hue, saturation, value)), 1);
}

float4 snowSideTex(float3 pos) {
	float blockPosY = (pos.y - 0.5) % 1;

	float bGh = 0.85;
	float gH = bGh - noise(pos, 10) * 0.1;
	
	
	if (blockPosY > gH)
		return snowTex(pos);
	return float4(0, 0, 0, 0);
}

float4 stoneTex(float3 pos) {
	return float4(1, 0, 1, 1);
}

// wip
// Generate texture for wood
float4 woodTex(float3 pos) {
	float hue = 0.083;
	float saturation = 0.5;
	float value = 0.8;

	float freq = 4;
	pos.xz *= 7;
	float v = noise(pos, freq) * noise(pos + 5124, freq - 2) * noise(pos + 661, freq + 1) * 0.3 + 0.1;
	value = clamp(v, 0, 1);


	return float4(HSVtoRGB(float3(hue, saturation, value)), 1);
}

float4 leafTex(float3 pos,float maxFreq) {
	float hue = 0.2;
	float saturation = 0.85;
	float value = 0.2;

	float f = 4;
	//float v = noise(pos, f) * 0.1 +
	//	noise(pos, f * 3) * 0.1 +
	//	noise(pos, f * 6) * 0.1 +
	//	noise(pos, f * 10) * 0.1 +
	//	noise(pos, f * 30) * 0.13 +
	//	noise(pos, f * 70) * 0.1 +
	//	noise(pos, f * 100) * 0.05 +
	//	noise(pos, f * 1.5) * noise(pos, f * 2) * 0.5;


	float noiseFreq[7] = {
		f,
		f * 3,
		f * 6,
		f * 10,
		f * 30,
		f * 70,
		f * 100
	};

	float noiseMag[7] = {
		0.1,
		0.1,
		0.1,
		0.1,
		0.13,
		0.1,
		0.05
	};

	if (f * 2 > maxFreq)
		value += noise(pos, f * 1.5) * noise(pos, f * 2) * 0.5;
	else
		value += 0.1;

	for (int i = 0; i < 7; i++) {
		if (noiseFreq[i] <= maxFreq)
			value += noise(pos, noiseFreq[i]) * noiseMag[i];
		else
			value += noiseMag[i] * 0.5;
	}





	value = clamp(value, 0, 1);

	return float4(HSVtoRGB(float3(hue, saturation, value)), 1);
}

float4  waterTex(float3 pos) {
	return float4(0.4, 0.4, 1, 0.95);
}


// Gets the value for a texel on a face 
float4 getTexel(int slice, float3 pos, float3 posEye) {
	float distFromEye = length(posEye);

	float maxFreq = 100;
	
	if (distFromEye > 110)
		maxFreq = 5;	
	else if (distFromEye > 60)
		maxFreq = 10;
	else if (distFromEye > 40)
		maxFreq = 30;
	else if (distFromEye > 15)
		maxFreq = 50;
	else if (distFromEye > 5)
		maxFreq = 80;	


	//return float4(maxFreq / 100.0, maxFreq / 100.0, maxFreq / 100.0, 1);

	switch (slice + 1) {
	case 1: // Dirt
		return dirtTex(pos);
	case 2: // Stone
		return stoneTex(pos);
	case 3: // Sand
		return sandTex(pos);
	case 4: // Grass top
		return grassTex(pos, maxFreq);
	case 5: // Grass side
		return grassSideTex(pos, maxFreq);
	case 6: // Snow top
		return snowTex(pos);
	case 7: // Snow side
		return snowSideTex(pos);
	case 8: // Wood
		return woodTex(pos);
	case 9: // Leaf
		return leafTex(pos, maxFreq);
	case 10: // Water
		return waterTex(pos);
	default:
		return float4(1, 1, 1, 0);
	}
}


#endif // __TEXTURES_HLSL__