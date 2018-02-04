#ifndef __UTIL_HLSL__
#define __UTIL_HLSL__

//Function for calulating specular light
float3 calcSpecular(float3 eyeLightDir, float3 eyeNormal, float3 eyePos, float exponent){
	float3 eyeReflection = reflect(-eyeLightDir, eyeNormal);
	float3 posToViewer = normalize(-eyePos);
	float dotSpecular = saturate(dot(eyeReflection, posToViewer));
	dotSpecular = max(dotSpecular, 0);
	float3 specular = pow((dotSpecular), exponent);
	return specular;
}


// Returns 1 if lower < input < upper, for 0 <= input <= 1. The functions purpose is to avoid code branching with if statements.  
float inRange (float input, float lower, float upper){
	return (ceil(input - lower) - ceil(input - upper));
}

//From looking at varius noise functions
// it looks like they're pretty much based on 
// some hash function, input coordinates and interpolation.
//this hash based noise function is based on https://www.shadertoy.com/view/XslGRr made by Inigo Quilez

float hash (float n){
	return frac(abs(sin(n.x) * 43758.5453));
}

float noise (float3 x){
	float3 p = floor(x); 
	float3 f = frac(x);
	f = f*f*(3.0-2.0*f);
	float n = p.x + p.y*57.0 + 113.0*p.z;
	//Interpolate hash values at the corners of the integral units cube
	float x1 = lerp(hash(n + 0.0), hash(n + 1.0), f.x);
	float x2 = lerp(hash(n + 57), hash(n + 58), f.x);
	float y1 = lerp(x1, x2, f.y);
	
	float x3 = lerp(hash(n + 113), hash(n + 114), f.x);
	float x4 = lerp(hash(n + 170), hash(n + 171), f.x);
	float y2 = lerp(x3, x4, f.y);
	
	return lerp(y1, y2, f.z);
}

float noise(float3 x, float freq) {
	return noise(x / freq);
}

/*
	A HLSL port of ingio quilez's noise function written in GLSL i found in a unity forum
	https://forum.unity.com/threads/perlin-noise-procedural-shader.33725/

float hash( float n )
{
    return frac(sin(n)*43758.5453);
}
 
float noise( float3 x )
{
    // The noise function returns a value in the range -1.0f -> 1.0f
 
    float3 p = floor(x);
    float3 f = frac(x);
 
    f       = f*f*(3.0-2.0*f);
    float n = p.x + p.y*57.0 + 113.0*p.z;
 
    return lerp(lerp(lerp( hash(n+0.0), hash(n+1.0),f.x),
                   lerp( hash(n+57.0), hash(n+58.0),f.x),f.y),
               lerp(lerp( hash(n+113.0), hash(n+114.0),f.x),
                   lerp( hash(n+170.0), hash(n+171.0),f.x),f.y),f.z);
}
*/

#endif // __UTILS_HLSL__