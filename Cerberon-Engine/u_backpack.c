#include <raylib.h>
#include "ui_manager.h"
#include "inventory.h"
#include "player.h"
#include "u_backpack.h"

static UIElement* UBackpackBG;
static UIElement* UBackpackSlots[1];

void UBackpackCreate()
{
	UBackpackBG = UIFindElement("InventoryBG");
	UBackpackBG->OnShow = UBackpackShow;
	UBackpackBG->OnDraw = UBackpackDraw;

	for (int i = 0; i < InventoryPlayerBack.MaxCount; i++)
	{
		UBackpackSlots[i] = UIFindElement(TextFormat("InventorySlot%d", i + 1));
	}
}

void UBackpackShow(UIElement* e)
{

}

void UBackpackDraw(UIElement* e)
{
	DrawRectangleRec(e->Rect.Rectangle, GRAY);

	for (int i = 0; i < InventoryPlayerBack.MaxCount; i++)
	{
		Rectangle r = UBackpackSlots[i]->Rect.Rectangle;
		DrawRectangleRec(r, UBackpackSlots[i]->Hovered? WHITE : DARKGRAY);
		DrawRectangleLines(r.x, r.y, r.width, r.height, UBackpackSlots[i]->Hovered ? DARKGRAY : GRAY);
	}
}