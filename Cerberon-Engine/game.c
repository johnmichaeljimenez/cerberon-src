#include <raylib.h>
#include "game.h"
#include "gamestate_mainmenu.h"
#include "gamestate_ingame.h"
#include "cursor.h"

void GameInit()
{
	CursorInit();
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
	CursorDraw();
}

void SetGameState(GameState* state)
{
	CursorChange(CURSORSTATE_None);

	if (CurrentGameState != NULL)
		CurrentGameState->OnUnload();

	CurrentGameState = state;
	CurrentGameState->OnLoad();
}