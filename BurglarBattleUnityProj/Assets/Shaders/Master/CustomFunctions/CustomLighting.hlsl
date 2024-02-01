#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED


#ifndef SHADERGRAPH_PREVIEW
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#if (SHADERPASS != SHADERPASS_FORWARD)
#undef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
#endif
#endif

void Shadowmask_half(float2 lightmapUV, out half4 Shadowmask) {
#ifdef SHADERGRAPH_PREVIEW
	Shadowmask = half4(1, 1, 1, 1);
#else
	OUTPUT_LIGHTMAP_UV(lightmapUV, unity_LightmapST, lightmapUV);
	Shadowmask = SAMPLE_SHADOWMASK(lightmapUV);
#endif
}


void MainLight_float(float3 WorldPos, out float3 Direction, out float3 Color, out float DistanceAtten, out float ShadowAtten)
{
#if SHADERGRAPH_PREVIEW
    Direction = float3(0.5, 0.5, 0);
    Color = 1;
    DistanceAtten = 1;
    ShadowAtten = 1;
#else
#if SHADOWS_SCREEN
    float4 clipPos = TransformWorldToHClip(WorldPos);
    float4 shadowCoord = ComputeScreenPos(clipPos);
#else
    float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
#endif
    Light mainLight = GetMainLight(shadowCoord);
    Direction = mainLight.direction;
    Color = mainLight.color;
    DistanceAtten = mainLight.distanceAttenuation;
    ShadowAtten = mainLight.shadowAttenuation;
#endif
}

void MainLight_half(float3 WorldPos, out half3 Direction, out half3 Color, out half DistanceAtten, out half ShadowAtten)
{
#if SHADERGRAPH_PREVIEW
    Direction = half3(0.5, 0.5, 0);
    Color = 1;
    DistanceAtten = 1;
    ShadowAtten = 1;
#else
#if SHADOWS_SCREEN
    half4 clipPos = TransformWorldToHClip(WorldPos);
    half4 shadowCoord = ComputeScreenPos(clipPos);
#else
    half4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
#endif
    Light mainLight = GetMainLight(shadowCoord);
    Direction = mainLight.direction;
    Color = mainLight.color;
    DistanceAtten = mainLight.distanceAttenuation;
    ShadowAtten = mainLight.shadowAttenuation;
#endif
}

void DirectSpecular_float(float3 Specular, float Smoothness, float3 Direction, float3 Color, float3 WorldNormal, float3 WorldView, out float3 Out)
{
#if SHADERGRAPH_PREVIEW
    Out = 0;
#else
    Smoothness = exp2(10 * Smoothness + 1);
    WorldNormal = normalize(WorldNormal);
    WorldView = SafeNormalize(WorldView);
    Out = LightingSpecular(Color, Direction, WorldNormal, WorldView, float4(Specular, 0), Smoothness);
#endif
}

void DirectSpecular_half(half3 Specular, half Smoothness, half3 Direction, half3 Color, half3 WorldNormal, half3 WorldView, out half3 Out)
{
#if SHADERGRAPH_PREVIEW
    Out = 0;
#else
    Smoothness = exp2(10 * Smoothness + 1);
    WorldNormal = normalize(WorldNormal);
    WorldView = SafeNormalize(WorldView);
    Out = LightingSpecular(Color, Direction, WorldNormal, WorldView, half4(Specular, 0), Smoothness);
#endif
}

//void AdditionalLights_float(float3 SpecColor, float Smoothness, float3 WorldPosition, float3 WorldNormal, float3 WorldView, out float3 Diffuse, out float3 Specular)
//{
//    float3 diffuseColor = 0;
//    float3 specularColor = 0;
//
//#ifndef SHADERGRAPH_PREVIEW
//    Smoothness = exp2(10 * Smoothness + 1);
//    WorldNormal = normalize(WorldNormal);
//    WorldView = SafeNormalize(WorldView);
//    int pixelLightCount = GetAdditionalLightsCount();
//
//
//    for (int i = 0; i < pixelLightCount; ++i)
//    {
//        Light light = GetAdditionalLight(i, WorldPosition);
//        half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
//        //diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal);
//        //specularColor += LightingSpecular(attenuatedLightColor, light.direction, WorldNormal, WorldView, float4(SpecColor, 0), Smoothness);
//    }
//#endif
//
//    Diffuse = diffuseColor;
//    Specular = specularColor;
//}
//
//void AdditionalLights_half(half3 SpecColor, half Smoothness, half3 WorldPosition, half3 WorldNormal, half3 WorldView, out half3 Diffuse, out half3 Specular)
//{
//    half3 diffuseColor = 0;
//    half3 specularColor = 0;
//
//#ifndef SHADERGRAPH_PREVIEW
//    Smoothness = exp2(10 * Smoothness + 1);
//    WorldNormal = normalize(WorldNormal);
//    WorldView = SafeNormalize(WorldView);
//    int pixelLightCount = GetAdditionalLightsCount();
//    for (int i = 0; i < pixelLightCount; ++i)
//    {
//        Light light = GetAdditionalLight(i, WorldPosition);
//        half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
//        diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal);
//        specularColor += LightingSpecular(attenuatedLightColor, light.direction, WorldNormal, WorldView, half4(SpecColor, 0), Smoothness);
//    }
//#endif
//
//    Diffuse = diffuseColor;
//    Specular = specularColor;
//}

#ifndef SHADERGRAPH_PREVIEW
float ToonAttenuation(int i, float3 positionWS, float pointBands, float spotBands) {
	int perObjectLightIndex = GetPerObjectLightIndex(i); // (i = index used in loop)
#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
	float4 lightPositionWS = _AdditionalLightsBuffer[perObjectLightIndex].position;
	half4 spotDirection = _AdditionalLightsBuffer[perObjectLightIndex].spotDirection;
	half4 distanceAndSpotAttenuation = _AdditionalLightsBuffer[perObjectLightIndex].attenuation;
#else
	float4 lightPositionWS = _AdditionalLightsPosition[perObjectLightIndex];
	half4 spotDirection = _AdditionalLightsSpotDir[perObjectLightIndex];
	half4 distanceAndSpotAttenuation = _AdditionalLightsAttenuation[perObjectLightIndex];
#endif

	// Point
	float3 lightVector = lightPositionWS.xyz - positionWS * lightPositionWS.w;
	float distanceSqr = max(dot(lightVector, lightVector), HALF_MIN);
	float range = rsqrt(distanceAndSpotAttenuation.x);
	float dist = sqrt(distanceSqr) / range;

	// Spot
	half3 lightDirection = half3(lightVector * rsqrt(distanceSqr));
	half SdotL = dot(spotDirection.xyz, lightDirection);
	half spotAtten = saturate(SdotL * distanceAndSpotAttenuation.z + distanceAndSpotAttenuation.w);
	spotAtten *= spotAtten;
	float maskSpotToRange = step(dist, 1);

	// Atten
	bool isSpot = (distanceAndSpotAttenuation.z > 0);
	return isSpot ?
		step(0.01, spotAtten) :		// cheaper for 1 band spot lights
		//(floor(spotAtten * spotBands) / spotBands) * maskSpotToRange :
		saturate(1.0 - floor(dist * pointBands) / pointBands);
}
#endif

void AdditionalLights_float(float3 SpecColor, float Smoothness, float3 WorldPosition, float3 WorldNormal, float3 WorldView, half4 Shadowmask,
	float PointLightBands, float SpotLightBands,
	out float3 Diffuse, out float3 Specular) {
	float3 diffuseColor = 0;
	float3 specularColor = 0;

#ifndef SHADERGRAPH_PREVIEW
	Smoothness = exp2(10 * Smoothness + 1);
	WorldNormal = normalize(WorldNormal);
	WorldView = SafeNormalize(WorldView);
	int pixelLightCount = GetAdditionalLightsCount();
	for (int i = 0; i < pixelLightCount; ++i) {
		Light light = GetAdditionalLight(i, WorldPosition, Shadowmask);

		if (PointLightBands <= 1 && SpotLightBands <= 1) {
			// Solid colour lights
			diffuseColor += light.color * step(0.0001, light.distanceAttenuation * light.shadowAttenuation);
		}
		else {
			// Multiple bands :
			diffuseColor += light.color * light.shadowAttenuation * ToonAttenuation(i, WorldPosition, PointLightBands, SpotLightBands);
		}
		
		float3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
		specularColor += LightingSpecular(attenuatedLightColor, light.direction, WorldNormal, WorldView, float4(SpecColor, 0), Smoothness);
	}
#endif

	Diffuse = diffuseColor;
	Specular = specularColor;
}

void AdditionalLights_float(float3 SpecColor, float Smoothness, float3 WorldPosition, float3 WorldNormal, float3 WorldView,
	float PointLightBands, float SpotLightBands,
	out float3 Diffuse, out float3 Specular) {
	AdditionalLights_float(SpecColor, Smoothness, WorldPosition, WorldNormal, WorldView, half4(1, 1, 1, 1),
		PointLightBands, SpotLightBands, Diffuse, Specular);
}

#endif