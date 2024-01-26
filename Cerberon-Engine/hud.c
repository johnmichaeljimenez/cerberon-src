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

	//inventory
	for (int i = 0; i < InventoryMaxSize; i++)
	{
		float y = 24 + (i * 32) + (i * 4);

		ItemPickup* in = &InventoryPlayer.Items[i];
		DrawRectangleLines(4, y, 32, 32, in->IsActive? WHITE : DARKGRAY);
		if (!in->IsActive)
			continue;

		DrawText(in->CurrentMaxAmount > 1 ? TextFormat("%d/%d", in->CurrentAmount, in->CurrentMaxAmount) : TextFormat("%d", in->CurrentAmount), 6, y + 2, 12, WHITE);
	}

	//misc
	DrawText(TimeGetString(), 4, size.y - 20, 12, WHITE);
}