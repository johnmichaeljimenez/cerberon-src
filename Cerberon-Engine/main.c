#include <raylib.h>
#include "game.h"
#include "log.h"
#include "time.h"
#include "audio_manager.h"

static int n = 0;
int main()
{
	float previous = GetTime();
	float lag = 0.0;

	ClearLog();

	SetConfigFlags(FLAG_VSYNC_HINT);
	InitWindow(1366, 768, "Cerberon Engine");
	AudioInit();

	TraceLog(LOG_INFO, "Current directory: %s", GetWorkingDirectory());

	GameInit();
	while (!WindowShouldClose())//&& n < 9999)
	{
		TICKRATE = GetFrameTime();
		InputUpdate();
		//float current = GetTime();
		//float elapsed = current - previous;
		//previous = current;
		//lag += elapsed;

		//while (lag >= TICKRATE)
		//{
		GameUpdate();
		//lag -= TICKRATE;
		//}

		AudioUpdate();
		BeginDrawing();

		ClearBackground(RAYWHITE);
		GameDraw();
		DrawFPS(2, 2);

		EndDrawing();
	}

	GameUnload();
	AudioUnload();
	CloseWindow();

	return 0;
}