#ifndef InventoryMaxSize
#define InventoryMaxSize 8
#endif // !InventoryMaxSize

#pragma once
#include <raylib.h>
#include <raymath.h>
#include "i_item.h"

typedef struct InventoryContainer
{
	ItemPickup* Items[8];
} InventoryContainer;

InventoryContainer InventoryPlayer;

void InventoryInit(InventoryContainer* in);
void InventoryUnload(InventoryContainer* in);
bool InventoryAdd(InventoryContainer* i, ItemPickup* item);
void InventoryUse(InventoryContainer* i, ItemPickup* item);
void InventoryDraw(InventoryContainer* i);
ItemPickup* InventoryGetItem(InventoryContainer* i, InteractableSubType itemType);