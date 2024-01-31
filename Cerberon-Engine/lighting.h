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
void DrawShadows(Light* light, bool useBounds, bool visionMode);
void DrawCircleShadows(Vector2 from, Vector2 circlePos, float circleRadius);
void DrawShadowsEx(Vector2 from, Vector2 to, Vector2 normal, Vector2 lightPos);
void DrawLightDefault(Light* l);

void UpdateLights(RenderTexture* screenRenderTexture, RenderTexture* effectsRenderTexture, RenderTexture* lightRenderTexture);
void DrawLights(RenderTexture* screenRenderTexture, RenderTexture* effectsRenderTexture, RenderTexture* lightRenderTexture);