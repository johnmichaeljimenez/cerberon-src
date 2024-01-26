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
	DrawRectangle(4, 4, (char)(120.0 * (PlayerEntity.Hitpoints / 100.0)), 15, (Color) { 120, 0, 0, 255 });
	DrawRectangleLines(4, 4, 120, 15, WHITE);

	//inventory
	for (int i = 0; i < InventoryMaxSize; i++)
	{
		float y = 24 + (i * 32) + (i * 4);

		ItemPickup* in = InventoryPlayer.Items[i];
		DrawRectangleLines(4, y, 32, 32, in == NULL ? DARKGRAY : WHITE);

		if (in == NULL)
			continue;

		char* amt;
		//if (in->CurrentMaxAmount > 1)
		//{
			//amt = TextFormat("%02d/%02d", in->CurrentAmount, in->CurrentMaxAmount);
		//}
		//else
		//{
			amt = TextFormat("%02d", in->CurrentAmount);
		//}

		//add icon here

		DrawText(amt, 6, y + 2, 8, WHITE);
	}

	//misc
	DrawText(TimeGetString(), 4, size.y - 20, 8, WHITE);
}