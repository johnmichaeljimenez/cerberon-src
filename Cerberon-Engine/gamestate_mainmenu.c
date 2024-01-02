#include <raylib.h>
#include "gamestate_mainmenu.h"

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

}

void MainMenuOnUnload()
{

}

void MainMenuOnUpdate()
{
	if (IsKeyPressed(KEY_SPACE))
		SetGameState(&GameStateIngame);
}

void MainMenuOnDraw()
{
	ClearBackground(DARKGRAY);
	DrawText("[SPACE] to start", 2, 20, 25, WHITE);
}