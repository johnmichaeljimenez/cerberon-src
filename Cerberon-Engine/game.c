#include <raylib.h>
#include "game.h"
#include "gamestate_mainmenu.h"
#include "gamestate_ingame.h"

void GameInit()
{
	MainMenuInit();
	IngameInit();

	CurrentGameState = NULL;
	SetGameState(&GameStateMainMenu);
}

void GameUnload()
{
	CurrentGameState->OnUnload();
}

void GameUpdate()
{
	CurrentGameState->OnUpdate();
}

void GameDraw()
{
	CurrentGameState->OnDraw();
}

void SetGameState(GameState* state)
{
	if (CurrentGameState != NULL)
		CurrentGameState->OnUnload();

	CurrentGameState = state;
	CurrentGameState->OnLoad();
}