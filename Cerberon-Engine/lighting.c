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

		DrawSprite(LightTexture, l->Position, l->Rotation, l->Scale/512, Vector2Zero());//  (l->Position, l->Scale / 2, l->Color);

		EndMode2D();
		EndTextureMode();
	}

	//DRAW AND BLEND LIGHTS
	BeginTextureMode(LightRenderTexture);
	BeginMode2D(GameCamera);

	ClearBackground(BLACK);

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
	BeginBlendMode(BLEND_ADDITIVE);
	DrawTexturePro(LightRenderTexture.texture, srcRec, destRect, origin, 0, DARKGRAY);
	EndBlendMode();
}