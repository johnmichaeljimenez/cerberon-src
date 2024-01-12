#include <raylib.h>
#include <raymath.h>
#include "player.h"
#include "i_item.h"
#include "mapdata_manager.h"
#include "memory.h"

void LoadItems()
{
	ItemList = MCalloc(ItemCount, sizeof(ItemPickup), "Item List");

	int n = 0;
	for (int i = 0; i < CurrentMapData->InteractableCount; i++)
	{
		Interactable* in = &CurrentMapData->Interactables[i];
		if (in->InteractableType != INTERACTABLE_Item)
			continue;

		ItemList[n].Interactable = in;
		ItemList[n].IsActive = true;
		ItemList[n].ItemStatusType = ITEMSTATUSTYPE_OnWorld;
		ItemList[n].CurrentAmount = in->Count;
		ItemList[n].CurrentMaxAmount = 1; //default
		ItemList[n].Index = n;

		if (in->InteractableSubType == INTERACTABLESUB_ItemMedkit)
		{
			ItemList[n].OnPickup = OnMedkitPickup;
			ItemList[n].OnUse = OnMedkitUse;
		}

		in->DataIndex = n;
		n++;
	}
}

void UnloadItems()
{
	MFree(ItemList, ItemCount, sizeof(ItemPickup), "Item List");
}

void ItemInit(Interactable* i)
{

}

void ItemUnload(Interactable* i)
{

}

void ItemUpdate(Interactable* i)
{
	ItemPickup* ip = &ItemList[i->DataIndex];
	if (ip->ItemStatusType != ITEMSTATUSTYPE_OnWorld)
		return;

}

void ItemLateUpdate(Interactable* i)
{
	ItemPickup* ip = &ItemList[i->DataIndex];
	if (ip->ItemStatusType != ITEMSTATUSTYPE_OnWorld)
		return;

}

void ItemDraw(Interactable* i)
{
	ItemPickup* ip = &ItemList[i->DataIndex];
	if (ip->ItemStatusType != ITEMSTATUSTYPE_OnWorld)
		return;

	DrawCircleV(i->Position, 32, BLUE);
}

void ItemInteract(Interactable* i, PlayerCharacter* p)
{
	ItemPickup* ip = &ItemList[i->DataIndex];
	ip->OnPickup(ip);
}

bool Pickup(ItemPickup* i)
{
	bool picked = i->OnUse(i);
	if (picked)
	{
		i->ItemStatusType = ITEMSTATUSTYPE_OnInventory;
	}
}

bool OnMedkitPickup(ItemPickup* i)
{
	bool used = i->OnUse(i);
	if (used)
	{
		i->ItemStatusType = ITEMSTATUSTYPE_None;
		i->IsActive = false;
		NextItemSlotIndex = i->Index;
	}

	return used;
}

bool OnMedkitUse(ItemPickup* i)
{
	//heal
}

bool OnFlashlightPickup(ItemPickup* i, PlayerCharacter* p)
{
	//toggle light
}

bool OnCashPickup(ItemPickup* i, PlayerCharacter* p)
{
	//add money
}