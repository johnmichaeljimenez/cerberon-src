#include <raylib.h>
#include "ui_manager.h"
#include "inventory.h"
#include "player.h"
#include "u_backpack.h"

static UIElement* UBackpackBG;
void UBackpackCreate()
{
	UBackpackBG = UIFindElement("InventoryBG");
	UBackpackBG->OnShow = UBackpackShow;
	UBackpackBG->OnDraw = UBackpackDraw;
}

void UBackpackShow(UIElement* e)
{

}

void UBackpackDraw(UIElement* e)
{

}