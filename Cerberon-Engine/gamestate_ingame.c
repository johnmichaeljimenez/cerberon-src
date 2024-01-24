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
#include "time.h"

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
	RendererPostInitialize();
	TimeInit();
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

void IngameOnUpdate()
{
	CursorChange(CURSORSTATE_IngameInteractReticle);
	UpdateMap(CurrentMapData);
	PlayerUpdate(&PlayerEntity);
	PlayerLateUpdate(&PlayerEntity);
	CameraUpdate();
	RendererUpdate();
	TimeUpdate();

	if (InputGetPressed(INPUTACTIONTYPE_UIBack))
		SetGameState(&GameStateMainMenu);
}

void IngameOnDraw()
{
	RendererDraw();
}