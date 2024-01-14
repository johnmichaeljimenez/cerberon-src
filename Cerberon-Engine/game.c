#include <raylib.h>
#include "game.h"
#include "gamestate_mainmenu.h"
#include "gamestate_ingame.h"
#include "cursor.h"
#include "input_handler.h"
#include "audio_manager.h"

void GameInit()
{
	AudioInit();
	CursorInit();
	MainMenuInit();
	IngameInit();

	CurrentGameState = NULL;
	SetGameState(&GameStateMainMenu);
}

void GameUnload()
{
	CurrentGameState->OnUnload();
	AudioUnload();
}

void GameUpdate()
{
	CursorUpdate();
	CurrentGameState->OnUpdate();
	AudioUpdate();
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