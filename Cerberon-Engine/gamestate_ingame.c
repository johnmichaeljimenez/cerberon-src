#include <raylib.h>
#include "game.h"
#include "gamestate_ingame.h"
#include "asset_manager.h"
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
	PlayerInit(&PlayerEntity);
	CameraSetTarget(PlayerEntity.Position, true);
}

void IngameOnUnload()
{
	PlayerUnload(&PlayerEntity);
	UnloadResources();
}

void IngameOnUpdate()
{
	PlayerUpdate(&PlayerEntity);
	PlayerLateUpdate(&PlayerEntity);

	CameraSetTarget(PlayerEntity.Position, false);
	CameraUpdate();
}

void IngameOnDraw()
{
	ClearBackground(BLACK);

	BeginMode2D(GameCamera);

	PlayerDraw(&PlayerEntity);

	EndMode2D();

	PlayerDrawHUD(&PlayerEntity);
}