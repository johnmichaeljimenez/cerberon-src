#include <raylib.h>
#include <raymath.h>
#include "lighting.h"
#include "camera.h"
#include "utils.h"
#include "asset_manager.h"
#include "mapdata_manager.h"

static RenderTexture2D LightRenderTexture;
static bool isLightingEnabled;

void InitLight()
{
	LightRenderTexture = LoadRenderTexture(GetScreenWidth(), GetScreenHeight());
	isLightingEnabled = true;
}

void UnloadLight()
{
	UnloadRenderTexture(LightRenderTexture);
}

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
		.offset = (Vector2){ sc / 2, sc / 2 }
	};

	light._RenderTexture = LoadRenderTexture(sc, sc);

	return light;
}

void UpdateLights()
{
	if (IsKeyPressed(KEY_F3))
		isLightingEnabled = !isLightingEnabled;

	if (!isLightingEnabled)
		return;

	//DRAW LIGHT RENDER TEXTURES
	//TODO: filter all static or non-shadow lights to render only once at initialization
	for (int i = 0; i < CurrentMapData->LightCount; i++)
	{
		Light* l = &CurrentMapData->Lights[i];
		l->_RenderCamera.target = l->Position;

		BeginTextureMode(l->_RenderTexture);
		ClearBackground(BLACK);
		BeginMode2D(l->_RenderCamera);

		Color color = ColorBrightness01(l->Color, l->Intensity);
		DrawSprite(LightTexture, l->Position, l->Rotation, l->Scale/512, Vector2Zero(), color);//  (l->Position, l->Scale / 2, l->Color);
		DrawShadows(l);

		EndMode2D();
		EndTextureMode();
	}

	//DRAW AND BLEND LIGHTS
	BeginTextureMode(LightRenderTexture);
	BeginMode2D(GameCamera);

	ClearBackground(ColorBrightness01(WHITE, 0.05));

	BeginBlendMode(BLEND_ADDITIVE);
	for (int i = 0; i < CurrentMapData->LightCount; i++)
	{
		Light* l = &CurrentMapData->Lights[i];
		Vector2 pos = l->Position;
		pos.x -= l->Scale / 2;
		pos.y -= l->Scale / 2;

		Rectangle srcRec = { 0, 0, l->_RenderTexture.texture.width, -(float)l->_RenderTexture.texture.height };
		Rectangle destRect = (Rectangle){ l->Position.x, l->Position.y, l->_RenderTexture.texture.width, l->_RenderTexture.texture.height };
		Vector2 origin = { l->_RenderTexture.texture.width / 2, l->_RenderTexture.texture.height / 2 };

		DrawTexturePro(l->_RenderTexture.texture, srcRec, destRect, origin, 0, WHITE);
	}
	EndBlendMode();

	EndMode2D();
	EndTextureMode();
}

void DrawLights()
{
	if (!isLightingEnabled)
		return;

	Texture2D* rt = &LightRenderTexture.texture;

	//DRAW ENTIRE LIGHT SCREEN QUAD
	BeginBlendMode(BLEND_MULTIPLIED);
	Rectangle srcRec = { 0, 0, rt->width, -(float)rt->height };
	Rectangle destRect = (Rectangle){ 0, 0, rt->width, rt->height };
	Vector2 origin = { 0,0 };
	DrawTexturePro(LightRenderTexture.texture, srcRec, destRect, origin, 0, WHITE);
	
	//FAKE VOLUME EFFECT
	//BeginBlendMode(BLEND_ADDITIVE);
	//DrawTexturePro(LightRenderTexture.texture, srcRec, destRect, origin, 0, DARKGRAY);
	EndBlendMode();
}

void DrawShadows(Light* light)
{
	if (!light->CastShadows)
		return;

	for (int i = 0; i < CurrentMapData->WallCount; i++)
	{
		Wall* w = &CurrentMapData->Walls[i];
		if (!HasFlag(w->WallFlags, WALLFLAG_CAST_SHADOW))
			continue;

		Vector2 d = Vector2Subtract(light->Position, w->Midpoint);
		bool visible = Vector2DotProduct(w->Normal, d) > 0;
		if (!visible)
			continue;

		w->sFrom = Vector2Add(w->From, Vector2Scale(Vector2Normalize(Vector2Subtract(w->From, light->Position)), 800));
		w->sTo = Vector2Add(w->To, Vector2Scale(Vector2Normalize(Vector2Subtract(w->To, light->Position)), 800));

		w->sFrom2 = Vector2Subtract(w->sFrom, Vector2Scale(w->Normal, 800));
		w->sTo2 = Vector2Subtract(w->sTo, Vector2Scale(w->Normal, 800));

		Vector2 points[6] = {
			w->From,
			w->To,
			w->sFrom,
			w->sTo,
			w->sFrom2,
			w->sTo2
		};

		DrawTriangleStrip(points, 6, BLACK);
	}
}