#include <raylib.h>
#include <raymath.h>
#include "lighting.h"

Light CreateLight(Vector2 pos, float rot, float sc, float intensity, Color color, bool cs)
{
	Light light = (Light){
		.Position = pos,
		.Rotation = rot,
		.Scale = sc,
		.Intensity = intensity,
		.Color = color,
		.CastShadows = cs
	};

	light._RenderCamera = (Camera2D){
		.zoom = 1,
		.target = pos,
	};

	light._RenderTexture = LoadRenderTexture(sc + 16, sc + 16);

	return light;
}