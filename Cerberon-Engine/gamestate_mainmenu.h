#pragma once
#include <raylib.h>
#include "game.h"
#include "gamestate_mainmenu.h"

GameState GameStateMainMenu;

void MainMenuInit();
void MainMenuOnLoad();
void MainMenuOnUnload();
void MainMenuOnUpdate();
void MainMenuOnDraw();