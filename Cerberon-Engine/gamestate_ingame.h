#pragma once
#include <raylib.h>
#include "game.h"

GameState GameStateIngame;

void IngameInit();
void IngameOnLoad();
void IngameOnUnload();
void IngameOnUpdate();
void IngameOnDraw();