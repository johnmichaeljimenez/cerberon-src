#pragma once
#include <raylib.h>
#include <raymath.h>
#include "i_item.h"

int InventoryMaxSize = 8;
typedef struct InventoryContainer
{
	ItemPickup* Items[8];
} InventoryContainer;

void InventoryAdd(InventoryContainer* i, ItemPickup* item);
void InventoryUse(InventoryContainer* i, ItemPickup* item);
void InventoryDraw(InventoryContainer* i);