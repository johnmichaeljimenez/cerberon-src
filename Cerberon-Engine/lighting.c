#include <raylib.h>
#include <raymath.h>
#include "lighting.h"
#include "camera.h"
#include "utils.h"
#include "asset_manager.h"
#include "mapdata_manager.h"
#include "i_door.h"

static RenderTexture2D LightRenderTexture;
static bool isLightingEnabled;
static float lightScale = 4;
static float screenLightScale = 4;
static Camera2D screenLightCamera;

static Shader blurShader;

void InitLight()
{
	blurShader = LoadShader(0, "res/gfx/lighting.frag");

	screenLightCamera = (Camera2D){
		.zoom = 1 / screenLightScale,
		.offset = (Vector2){ GetScreenWidth() / 2 / screenLightScale, GetScreenHeight() / 2 / screenLightScale }
	};

	LightRenderTexture = LoadRenderTexture(GetScreenWidth() / screenLightScale, GetScreenHeight() / screenLightScale);
	isLightingEnabled = true;
}

void UnloadLight()
{
	UnloadShader(blurShader);
	UnloadRenderTexture(LightRenderTexture);
}

Light CreateLight(Vector2 pos, float rot, float sc, float intensity, Color color, bool cs, void* drawCommand)
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
		.zoom = 1 / lightScale,
		.target = pos,
		.offset = (Vector2){ sc / 2 / lightScale, sc / 2 / lightScale }
	};

	light._RenderTexture = LoadRenderTexture(sc, sc);
	light.OnDrawLight = drawCommand;

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
		l->OnDrawLight(l);
		DrawShadows(l);

		EndMode2D();
		EndTextureMode();
	}

	//DRAW AND BLEND LIGHTS

	screenLightCamera.target = GameCamera.target;

	BeginTextureMode(LightRenderTexture);
	BeginMode2D(screenLightCamera);

	ClearBackground(ColorBrightness01(WHITE, 0.05));

	BeginBlendMode(BLEND_ADDITIVE);
	for (int i = 0; i < CurrentMapData->LightCount; i++)
	{
		Light* l = &CurrentMapData->Lights[i];
		Vector2 pos = l->Position;
		pos.x -= l->Scale / 2;
		pos.y -= l->Scale / 2;

		Rectangle srcRec = { 0, 0, l->_RenderTexture.texture.width, -(float)l->_RenderTexture.texture.height };
		Rectangle destRect = (Rectangle){ l->Position.x, l->Position.y, l->_RenderTexture.texture.width * lightScale, l->_RenderTexture.texture.height * lightScale };
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

	//DRAW ENTIRE LIGHT SCREEN QUAD
	Texture2D* rt = &LightRenderTexture.texture;

	BeginShaderMode(blurShader);
	//BeginBlendMode(BLEND_MULTIPLIED);
	Rectangle srcRec = { 0, 0, rt->width, -(float)rt->height };
	Rectangle destRect = (Rectangle){ 0, 0, rt->width * screenLightScale , rt->height * screenLightScale };
	Vector2 origin = { 0,0 };
	DrawTexturePro(LightRenderTexture.texture, srcRec, destRect, origin, 0, WHITE);
	EndShaderMode();

	//EndBlendMode();
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