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
#include "weapon_manager.h"
#include "projectile.h"
#include "ui_manager.h"
#include "dialogue_manager.h"
#include "character_entity.h"
#include "prop.h"

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
	PropInit();
	InitMap();
	WeaponInitData();
	InteractionInit();
	ProjectileInit();
	CharacterInit();
	CameraSetTarget(PlayerEntity->Position, true);
	RendererPostInitialize();
	TimeInit();
	UIInit();
}

void IngameOnUnload()
{
	InteractionUnload();
	PropUnload();
	UnloadLight();
	PlayerUnload(&PlayerEntity);
	CharacterUnload();
	UnloadMap();
	RendererUnload();
	UnloadResources();
}

void IngameOnUpdate()
{
	CursorChange(CURSORSTATE_IngameInteractReticle);
	UpdateMap(CurrentMapData);
	CharacterUpdate();
	ProjectileUpdate();
	PropUpdate();
	CameraUpdate();
	RendererUpdate();
	TimeUpdate();
	UIUpdate();

	if (IsKeyDown(KEY_TAB))
		DialogueShow("test", NULL);

	if (InputGetPressed(INPUTACTIONTYPE_UIBack))
		SetGameState(&GameStateMainMenu);
}

void IngameOnDraw()
{
	RendererDraw();
	UIDraw();
}