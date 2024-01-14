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

	TilesDraw();
	PlayerDraw(&PlayerEntity);
	DrawMap(CurrentMapData);

	EndMode2D();
	EndTextureMode();

	PlayerDrawHUD(&PlayerEntity);
	DrawMapHUD(CurrentMapData);
}