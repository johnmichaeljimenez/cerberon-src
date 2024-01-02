#include <raylib.h>
#include "game.h"

int main()
{
	SetConfigFlags(FLAG_VSYNC_HINT);
	InitWindow(1366, 768, "Cerberon Engine");

	GameInit();
	while (!WindowShouldClose())
	{
		GameUpdate();

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