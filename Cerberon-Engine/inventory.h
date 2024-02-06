#ifndef InventoryMaxSize
#define InventoryMaxSize 32
#endif // !InventoryMaxSize

#pragma once
#include <raylib.h>
#include <raymath.h>
#include "i_item.h"

typedef struct InventoryContainer
{
	ItemPickup* Items[InventoryMaxSize];
	int MaxCount;
	int CurrentSelectedIndex;
} InventoryContainer;

InventoryContainer InventoryPlayer;
InventoryContainer InventoryPlayerBack;

void InventoryInit(InventoryContainer* in, int maxCount);
void InventoryUnload(InventoryContainer* in);
bool InventoryAdd(InventoryContainer* i, ItemPickup* item);
void InventoryUse(InventoryContainer* i, ItemPickup* item);
void InventoryDraw(InventoryContainer* i);
ItemPickup* InventoryGetItem(InventoryContainer* i, InteractableSubType itemType);