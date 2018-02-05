#ifndef __TEXTURES_HLSL__
#define __TEXTURES_HLSL__

#include "utils.hlsl"

/*
	Experimental!
	Porting TerrainTextureGenerator from C# to HLSL for more efficient resource distribution.
	Doing texture generation on GPU is better for various reasons:
	1. More variety. No more choosing between pre-generated textures, textures are now generated per texel in the world.
	2. Better distribution of resources. The CPU is already busy with terrain generataion, and the GPU is doing almost nothing.

*/



float4 grassTex(float3 pos) {
	float hue = 0.2f;
	float saturation = 0.85f;
	float value = 0.6f;


	float v = noise(pos * 80);
	v += noise(pos * 40);
	v += noise(pos * 10);
	v += noise(pos * 120);
	v += noise(pos * 200);

	float f = 2;// 2 / 0.004;
	v = noise(pos, f) * 0.1 +
		noise(pos, f * 3) * 0.1 +
		noise(pos, f * 6) * 0.1 +
		noise(pos, f * 10) * 0.1 +
		noise(pos, f * 30) * 0.15 +
		noise(pos, f * 70) * 0.15 +
		noise(pos, f * 100) * 0.15;


	value = clamp(value *0.5 + v * 0.5f, 0, 1);

	float4 o = float4(HSVtoRGB(float3(hue, saturation, value)), 1);

	return o;
}

float4 grassSideTex(float3 pos) {
	float blockPosY = (pos.y - 0.5) % 1;

	float bGh = 0.85;

	float gH = bGh - noise(pos, 80) * 0.1;


	if(blockPosY > gH)
		return grassTex(pos);
	return float4(0, 0, 0, 0);
}

float4 dirtTex(float3 pos) {
	return float4(1, 1, 1, 2);
}
float4 sandTex(float3 pos) {
	return float4(0, 0, 0, 2);
}
float4 snowTex(float3 pos) {
	return float4(0, 0, 0, 2);
}
float4 snowSideTex(float3 pos) {
	return float4(0, 0, 0, 2);
}
float4 stoneTex(float3 pos) {
	return float4(0, 0, 0, 2);
}

//wip
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
float4 leafTex(float3 pos) {
	return float4(0, 0, 0, 2);
}




float4 getTexel(int slice, float3 pos) {
	switch (slice + 1) {
	case 1: // Dirt
		return dirtTex(pos);
	case 2: // Stone
		return stoneTex(pos);
	case 3: // Sand
		return sandTex(pos);	
	case 4: // Grass top
		return grassTex(pos);
	case 5: // Grass side
		return grassSideTex(pos);
	case 6: // Snow top
		return snowTex(pos);
	case 7: // Snow side
		return snowSideTex(pos);
	case 8: // Wood
		return woodTex(pos);
	case 9: // Leaf
		return leafTex(pos);	
	default:
		return float4(1, 1, 1, 0);
	}
}


#endif // __TEXTURES_HLSL__