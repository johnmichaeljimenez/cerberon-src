#include <raylib.h>

int main()
{
	SetConfigFlags(FLAG_VSYNC_HINT);
	InitWindow(1366, 768, "Cerberon Engine");

	while (!WindowShouldClose())
	{
		BeginDrawing();

		ClearBackground(RAYWHITE);
		DrawFPS(2, 2);

		EndDrawing();
	}

	CloseWindow();

	return 0;
}