#pragma once

typedef struct GameState
{
	void(*OnLoad)();
	void(*OnUnload)();
	void(*OnUpdate)();
	void(*OnDraw)();

} GameState;

GameState* CurrentGameState;

void GameInit();
void GameUnload();
void GameUpdate();
void GameDraw();
void SetGameState(GameState* state);