#include <raylib.h>
#include <raymath.h>
#include "player.h"
#include "i_item.h"

//ItemPickup CreateItem(Vector2 pos, float rot, ItemType type, int index)
//{
//	ItemPickup item = { 0 };
//	item.Position = pos;
//	item.Rotation = rot;
//	item.ItemType = type;
//	item.Count = 1;
//	item.IsActive = true;
//	item.Index = index;
//
//	switch (type)
//	{
//	case ITEMTYPE_MEDKIT:
//		break;
//	case ITEMTYPE_FLASHLIGHT:
//		break;
//	case ITEMTYPE_CASH:
//		break;
//	}
//
//	return item;
//}
//
//bool Pickup(ItemPickup* i, PlayerCharacter* p)
//{
//	bool used = i->OnUse(i, p);
//	if (used)
//	{
//		i->IsActive = false;
//		NextItemSlotIndex = i->Index;
//	}
//
//	return used;
//}
//
//bool OnMedkitPickup(ItemPickup* i, PlayerCharacter* p)
//{
//	//heal
//}
//
//bool OnFlashlightPickup(ItemPickup* i, PlayerCharacter* p)
//{
//	//toggle light
//}
//
//bool OnCashPickup(ItemPickup* i, PlayerCharacter* p)
//{
//	//add money
//}