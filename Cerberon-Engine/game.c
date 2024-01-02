#include <raylib.h>
#include "game.h"
#include "gamestate_mainmenu.h"
#include "gamestate_ingame.h"

void GameInit()
{
	MainMenuInit();
	IngameInit();

	CurrentGameState = NULL;
	SetGameState(&GameStateIngame);
}

void GameUnload()
{
	CurrentGameState->OnUnload();
}

void GameUpdate()
{
	CurrentGameState->OnUpdate();

	if (IsKeyPressed(KEY_F1))
	{
		if (CurrentGameState == &GameStateIngame)
			SetGameState(&GameStateMainMenu);
		else
			SetGameState(&GameStateIngame);
	}
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