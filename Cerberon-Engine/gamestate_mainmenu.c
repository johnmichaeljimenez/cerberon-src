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

}

void MainMenuOnDraw()
{
	ClearBackground(RED);
}