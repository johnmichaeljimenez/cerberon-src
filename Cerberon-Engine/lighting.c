#include <raylib.h>
#include <raymath.h>
#include "lighting.h"
#include "camera.h"
#include "utils.h"
#include "asset_manager.h"
#include "mapdata_manager.h"
#include "i_door.h"
#include "renderer.h"
#include "camera.h"

static float lightScale = 4;
static float screenLightScale = 2;
static Camera2D screenLightCamera;

void InitLight()
{
	LightAmbientColor = ColorBrightness01(WHITE, 0);

	screenLightCamera = (Camera2D){
		.zoom = 1 / screenLightScale,
		.offset = (Vector2){ GetScreenWidth() / 2 / screenLightScale, GetScreenHeight() / 2 / screenLightScale }
	};
}

void UnloadLight()
{

}

void CreateLight(Light* light)
{
	if (light->CastShadows)
	{
		light->_RenderCamera = (Camera2D){
			.zoom = 1 / lightScale,
			.target = light->Position,
			.offset = (Vector2){ light->Scale / 2 / lightScale, light->Scale / 2 / lightScale }
		};

		light->_RenderTexture = LoadRenderTexture(light->Scale, light->Scale);
	}

	UpdateLightBounds(light);
}

void UpdateLightBounds(Light* l)
{
	float size = l->Scale;
	l->_Bounds = (Rectangle){ l->Position.x - (size / 2), l->Position.y - (size / 2), size, size };
}

void UpdateLights(RenderTexture* screenRenderTexture, RenderTexture* effectsRenderTexture, RenderTexture* lightRenderTexture)
{
	//DRAW LIGHT RENDER TEXTURES
	for (int i = 0; i < CurrentMapData->LightCount; i++)
	{
		Light* l = &CurrentMapData->Lights[i];
		if (!l->AlwaysOn && !CheckCollisionRecs(l->_Bounds, CameraViewBounds))
			continue;

		if (!l->CastShadows)
			continue;

		l->_RenderCamera.target = l->Position;

		BeginTextureMode(l->_RenderTexture);
		ClearBackground(BLACK);
		BeginMode2D(l->_RenderCamera);
		l->OnDrawLight(l);
		DrawShadows(l, i > 0);

		EndMode2D();
		EndTextureMode();
	}

	//DRAW AND BLEND LIGHTS

	screenLightCamera.target = GameCamera.target;

	BeginTextureMode(*lightRenderTexture);
	BeginMode2D(screenLightCamera);

	ClearBackground(LightAmbientColor);

	BeginBlendMode(BLEND_ADDITIVE);
	for (int i = 0; i < CurrentMapData->LightCount; i++)
	{
		Light* l = &CurrentMapData->Lights[i];
		if (!l->AlwaysOn && !CheckCollisionRecs(l->_Bounds, CameraViewBounds))
			continue;

		if (!l->CastShadows)
		{
			l->OnDrawLight(l);
			continue;
		}

		Vector2 pos = l->Position;
		pos.x -= l->Scale / 2;
		pos.y -= l->Scale / 2;

		Rectangle srcRec = { 0, 0, l->_RenderTexture.texture.width, -(float)l->_RenderTexture.texture.height };
		Rectangle destRect = (Rectangle){ l->Position.x, l->Position.y, l->_RenderTexture.texture.width * lightScale, l->_RenderTexture.texture.height * lightScale };
		Vector2 origin = { l->_RenderTexture.texture.width / 2, l->_RenderTexture.texture.height / 2 };

		DrawTexturePro(l->_RenderTexture.texture, srcRec, destRect, origin, 0, WHITE);
	}

	for (int i = 0; i < CurrentMapData->TriggerCount; i++)
	{
		Trigger* t = &CurrentMapData->Triggers[i];
		if (!t->HasAmbientLight)
			continue;

		for (int j = 0; j < t->ColliderCount; j++)
		{
			TriggerCollider* c = &t->Colliders[j];
			DrawRectanglePro(c->_rectangle, (Vector2) { c->_rectangle.width / 2, c->_rectangle.height / 2 }, c->Rotation * RAD2DEG, t->AmbientLightColor);
		}
	}

	EndBlendMode();

	DrawWalls();
	EndMode2D();
	EndTextureMode();

	BeginTextureMode(*effectsRenderTexture);
	BeginMode2D(screenLightCamera);

	ClearBackground(BLACK);

	DrawPlayerVision();

	DrawShadows(&CurrentMapData->Lights[0], false);

	DrawWalls();
	EndMode2D();
	EndTextureMode();
}

void DrawLights(RenderTexture* screenRenderTexture, RenderTexture* effectsRenderTexture, RenderTexture* lightRenderTexture)
{
	//DRAW ENTIRE LIGHT SCREEN QUAD

	//DrawRenderTextureToScreen(&lightRenderTexture->texture, screenLightScale);
}

void DrawShadows(Light* light, bool useBounds)
{
	if (!light->CastShadows)
		return;

	for (int i = 0; i < CurrentMapData->WallCount; i++)
	{
		Wall* w = &CurrentMapData->Walls[i];
		if (!HasFlag(w->WallFlags, WALLFLAG_CAST_SHADOW))
			continue;

		if (useBounds && !CheckCollisionRecs(light->_Bounds, w->_Bounds))
			continue;

		Vector2 d = Vector2Subtract(light->Position, w->Midpoint);
		bool visible = Vector2DotProduct(w->Normal, d) > 0;
		if (!visible)
			continue;

		DrawShadowsEx(w->From, w->To, w->Normal, light->Position);
	}

	for (int i = 0; i < CurrentMapData->InteractableCount; i++)
	{
		Interactable* in = &CurrentMapData->Interactables[i];
		if (in->InteractableType != INTERACTABLE_Door)
			continue;

		Door* door = &DoorList[in->DataIndex];

		//Paper-thin 2 line segments (2nd line is just inverted normal)
		//TODO: improve this dirty hack, use polygons next time
		Vector2 normal = GetNormalVector(door->From, door->To);
		Vector2 normal2 = GetNormalVector(door->To, door->From);

		Vector2 d = Vector2Subtract(light->Position, door->DoorPosition);
		bool visible = Vector2DotProduct(normal, d) > 0;
		if (!visible)
			DrawShadowsEx(door->To, door->From, normal2, light->Position);
		else
			DrawShadowsEx(door->From, door->To, normal, light->Position);
	}
}

void DrawShadowsEx(Vector2 from, Vector2 to, Vector2 normal, Vector2 lightPos)
{
	Vector2 sFrom = Vector2Add(from, Vector2Scale(Vector2Normalize(Vector2Subtract(from, lightPos)), 800));
	Vector2 sTo = Vector2Add(to, Vector2Scale(Vector2Normalize(Vector2Subtract(to, lightPos)), 800));

	Vector2 sFrom2 = Vector2Subtract(sFrom, Vector2Scale(normal, 800));
	Vector2 sTo2 = Vector2Subtract(sTo, Vector2Scale(normal, 800));

	Vector2 points[6] = {
		from,
		to,
		sFrom,
		sTo,
		sFrom2,
		sTo2
	};

	DrawTriangleStrip(points, 6, BLACK);
}

void DrawLightDefault(Light* l)
{
	Color color = ColorBrightness01(l->Color, l->Intensity);
	DrawCircleGradient(l->Position.x, l->Position.y, l->Scale / 2, color, BLACK);
}