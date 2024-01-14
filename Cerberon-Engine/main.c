#include <raylib.h>
#include "game.h"
#include "log.h"
#include "time.h"

static int n = 0;
int main()
{
	float previous = GetTime();
	float lag = 0.0;

	ClearLog();

	SetConfigFlags(FLAG_VSYNC_HINT);
	InitWindow(1366, 768, "Cerberon Engine");

	TraceLog(LOG_INFO, "Current directory: %s", GetWorkingDirectory());

	GameInit();
	while (!WindowShouldClose() && n < 9999)
	{
		InputUpdate();
		float current = GetTime();
		float elapsed = current - previous;
		previous = current;
		lag += elapsed;

		while (lag >= TICKRATE)
		{
			GameUpdate();
			lag -= TICKRATE;
		}

		BeginDrawing();

		ClearBackground(RAYWHITE);
		GameDraw();
		DrawFPS(2, 2);

		EndDrawing();
	}

	GameUnload();
	CloseWindow();
	
	return 0;
}