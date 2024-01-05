#pragma once
#include <raylib.h>
#include <raymath.h>

typedef struct Light
{
	Vector2 Position;
	float Rotation;
	float Scale;

	float Intensity;
	Color Color;
	bool CastShadows;

	Camera2D _RenderCamera;
	RenderTexture2D _RenderTexture;

} Light;

Light CreateLight(Vector2 pos, float rot, float sc, float intensity, Color color, bool cs);
void UpdateLights(int lightCount, Light** lightArray);
void DrawLights(int lightCount, Light** lightArray);