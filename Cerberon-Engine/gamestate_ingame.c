#include <raylib.h>
#include "game.h"
#include "gamestate_ingame.h"
#include "asset_manager.h"
#include "mapdata_manager.h"
#include "camera.h"
#include "player.h"

void IngameInit()
{
	GameStateIngame.OnLoad = IngameOnLoad;
	GameStateIngame.OnUnload = IngameOnUnload;
	GameStateIngame.OnUpdate = IngameOnUpdate;
	GameStateIngame.OnDraw = IngameOnDraw;
}

void IngameOnLoad()
{
	LoadResources();
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
	UnloadResources();
}

void IngameOnUpdate()
{
	UpdateMap(CurrentMapData);
	PlayerUpdate(&PlayerEntity);
	PlayerLateUpdate(&PlayerEntity);
	CameraUpdate();

	if (IsKeyPressed(KEY_F1))
		SetGameState(&GameStateMainMenu);
}

void IngameOnDraw()
{
	ClearBackground(DARKGRAY);

	BeginMode2D(GameCamera);

	PlayerDraw(&PlayerEntity);
	DrawMap(CurrentMapData);

	EndMode2D();

	PlayerDrawHUD(&PlayerEntity);
	DrawMapHUD(CurrentMapData);
}