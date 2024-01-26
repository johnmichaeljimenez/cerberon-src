#include <raylib.h>
#include <raymath.h>
#include "hud.h"
#include "time.h"
#include "inventory.h"
#include "player.h"

void DrawHUD()
{
	Vector2 size = (Vector2){ GetScreenWidth(), GetScreenHeight() };

	//player stats
	DrawRectangle(4, 4, 120, 15, (Color) { 25, 25, 25, 255 });
	DrawRectangle(4, 4, (char)(120.0 * (PlayerEntity.Hitpoints / 100.0)), 15, (Color){120, 0, 0, 255 });
	DrawRectangleLines(4, 4, 120, 15, WHITE);

	//misc
	DrawText(TimeGetString(), 4, size.y - 20, 12, WHITE);
}