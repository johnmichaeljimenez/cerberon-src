#include <raylib.h>
#include "game.h"
#include "gamestate_ingame.h"
#include "asset_manager.h"

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
}

void IngameOnUnload()
{
	UnloadResources();
}

void IngameOnUpdate()
{

}

void IngameOnDraw()
{
	ClearBackground(BLACK);
}