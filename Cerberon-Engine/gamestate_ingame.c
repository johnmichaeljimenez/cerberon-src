#include <raylib.h>
#include "game.h"
#include "gamestate_ingame.h"
#include "asset_manager.h"
#include "mapdata_manager.h"
#include "camera.h"
#include "player.h"
#include "renderer.h"
#include "cursor.h"
#include "utils.h"
#include "input_handler.h"

void IngameInit()
{
	GameStateIngame.OnLoad = IngameOnLoad;
	GameStateIngame.OnUnload = IngameOnUnload;
	GameStateIngame.OnUpdate = IngameOnUpdate;
	GameStateIngame.OnDraw = IngameOnDraw;
}

void IngameOnLoad()
{
	CursorChange(CURSORSTATE_IngameInteractReticle);
	LoadResources();
	RendererInit();
	CameraInit();
	InitLight();
	InitMap();
	InteractionInit();
	PlayerInit(&PlayerEntity);
	CameraSetTarget(PlayerEntity.Position, true);
}

void IngameOnUnload()
{
	InteractionUnload();
	UnloadLight();
	PlayerUnload(&PlayerEntity);
	UnloadMap();
	RendererUnload();
	UnloadResources();
}

int nn = 0;
void IngameOnUpdate()
{
	CursorChange(CURSORSTATE_IngameInteractReticle);
	UpdateMap(CurrentMapData);
	PlayerUpdate(&PlayerEntity);
	PlayerLateUpdate(&PlayerEntity);
	CameraUpdate();

	if (InputGetPressed(INPUTACTIONTYPE_UIBack))
		SetGameState(&GameStateMainMenu);

	if (InputGetPressed(INPUTACTIONTYPE_Flashlight))
	{
		TraceLog(LOG_INFO, "PRESS! %d", nn);
		nn++;
	}
}

void IngameOnDraw()
{
	BeginTextureMode(RendererScreenTexture);
	ClearBackground(DARKGRAY);

	BeginMode2D(GameCamera);

	TextureResource* t = GetTextureResource(ToHash("wood_planks_3"));

	DrawTexturePro(t->Texture, (Rectangle) { 0, 0, t->Texture.width, t->Texture.height }, (Rectangle) { -4096, -4096, 8192, 8192 }, Vector2Zero(), 0, WHITE);

	/*for (int xx = -4096; xx < 4096; xx+=64)
	{
		for (int yy = -4096; yy < 4096; yy+=64)
		{
			DrawTexture(t->Texture, xx, yy, WHITE);
		}
	}*/

	PlayerDraw(&PlayerEntity);
	DrawMap(CurrentMapData);

	EndMode2D();
	EndTextureMode();

	PlayerDrawHUD(&PlayerEntity);
	DrawMapHUD(CurrentMapData);
}