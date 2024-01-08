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

	void(*OnDrawLight)(struct Light* l);

} Light;

void InitLight();
void UnloadLight();
Light CreateLight(Vector2 pos, float rot, float sc, float intensity, Color color, bool cs, void* drawCommand);
void UpdateLights();
void DrawLights();
void DrawShadows(Light* light);
void DrawShadowsEx(Vector2 from, Vector2 to, Vector2 normal, Vector2 lightPos);
void DrawLightDefault(Light* l);