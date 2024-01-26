#include <raylib.h>
#include "game.h"
#include "log.h"
#include "time.h"
#include "audio_manager.h"

int main()
{
	ClearLog();

	//SetConfigFlags(FLAG_VSYNC_HINT);
	InitWindow(1366, 768, "Cerberon Engine");
	AudioInit();

	TraceLog(LOG_INFO, "Current directory: %s", GetWorkingDirectory());

	GameInit();
	while (!WindowShouldClose())
	{
		TICKRATE = GetFrameTime();
		GameUpdate();

		AudioUpdate();
		BeginDrawing();

		ClearBackground(RAYWHITE);
		GameDraw();

		EndDrawing();
	}

	GameUnload();
	AudioUnload();
	CloseWindow();

	return 0;
}