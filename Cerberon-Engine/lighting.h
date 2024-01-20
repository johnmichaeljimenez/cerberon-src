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
	bool AlwaysOn;

	Camera2D _RenderCamera;
	RenderTexture2D _RenderTexture;
	Rectangle _Bounds;

	void(*OnDrawLight)(struct Light* l);

} Light;

Color LightAmbientColor;

void InitLight();
void UnloadLight();
void CreateLight(Light* light);
void UpdateLightBounds(Light* l);
void UpdateLights();
void DrawLights();
void DrawShadows(Light* light);
void DrawShadowsEx(Vector2 from, Vector2 to, Vector2 normal, Vector2 lightPos);
void DrawLightDefault(Light* l);