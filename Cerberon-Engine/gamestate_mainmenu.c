#include <raylib.h>
#include "gamestate_mainmenu.h"
#include "cursor.h"
#include "input_handler.h"

void MainMenuInit()
{
	GameStateMainMenu = (GameState){ 0 };

	GameStateMainMenu.OnLoad = MainMenuOnLoad;
	GameStateMainMenu.OnUnload = MainMenuOnUnload;
	GameStateMainMenu.OnUpdate = MainMenuOnUpdate;
	GameStateMainMenu.OnDraw = MainMenuOnDraw;
}

void MainMenuOnLoad()
{
	CursorChange(CURSORSTATE_Menu);
}

void MainMenuOnUnload()
{

}

void MainMenuOnUpdate()
{
	if (InputGetPressed(INPUTACTIONTYPE_UIConfirm))
		SetGameState(&GameStateIngame);
}

void MainMenuOnDraw()
{
	ClearBackground(DARKGRAY);
	DrawText("[SPACE] to start", GetScreenWidth()/2, GetScreenHeight()-100, 25, WHITE);
}