#pragma once
#include <raylib.h>
#include <raymath.h>
#include "player.h"

typedef enum ItemType
{
	ITEMTYPE_MEDKIT,
	ITEMTYPE_FLASHLIGHT,
	ITEMTYPE_CASH
} ItemType;

typedef struct ItemPickup
{
	Vector2 Position;
	float Rotation;
	ItemType ItemType;
	int Index;

	bool IsActive;
	int Count;
	bool (*OnUse)(struct ItemPickup* i);

} ItemPickup;

int NextItemSlotIndex;

ItemPickup CreateItem(Vector2 pos, float rot, ItemType type, int index);
bool Pickup(ItemPickup* i);
bool OnMedkitPickup(ItemPickup* i);