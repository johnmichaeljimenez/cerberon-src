#include <raylib.h>
#include "game.h"
#include "gamestate_ingame.h"

void IngameInit()
{
	GameStateIngame.OnLoad = IngameOnLoad;
	GameStateIngame.OnUnload = IngameOnUnload;
	GameStateIngame.OnUpdate = IngameOnUpdate;
	GameStateIngame.OnDraw = IngameOnDraw;
}

void IngameOnLoad()
{

}

void IngameOnUnload()
{

}

void IngameOnUpdate()
{

}

void IngameOnDraw()
{
	ClearBackground(BLUE);
}