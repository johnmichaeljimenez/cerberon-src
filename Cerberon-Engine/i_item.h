#pragma once
#include <raylib.h>
#include <raymath.h>
#include "player.h"

typedef enum ItemType
{
	ITEMTYPE_MEDKIT,
	ITEMTYPE_FLASHLIGHT,
	ITEMTYPE_CASH
};

typedef struct ItemPickup
{
	Vector2 Position;
	float Rotation;
	ItemType ItemType;

	bool IsActive;
	int Count;
	bool(*OnUse)(ItemPickup* i, PlayerCharacter* p);

} ItemPickup;

ItemPickup CreateItem(Vector2 pos, float rot, ItemType type)
{
	ItemPickup item = { 0 };
	item.Position = pos;
	item.Rotation = rot;
	item.ItemType = type;
	item.Count = 1;

	switch (type)
	{
	case ITEMTYPE_MEDKIT:
		break;
	case ITEMTYPE_FLASHLIGHT:
		break;
	case ITEMTYPE_CASH:
		break;
	}

	return item;
}