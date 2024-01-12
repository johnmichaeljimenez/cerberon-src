#include "inventory.h"

bool InventoryAdd(InventoryContainer* ic, ItemPickup* item)
{
	ItemPickup** a = ic->Items;

	for (int i = 0; i < InventoryMaxSize; i++)
	{
		ItemPickup* ip = a[i];
		if (ip == NULL || ip->ItemStatusType == ITEMSTATUSTYPE_None) //empty slot
		{
			a[i] = item;
			TraceLog(LOG_INFO, "ITEM PICK %d (empty) %d", i, item->Interactable->Count);
			return true;
		}

		Interactable* in = item->Interactable;
		if (ip->Interactable->InteractableSubType == item->Interactable->InteractableSubType && item->CurrentMaxAmount > 1) //same type, should stack if allowed
		{
			ip->Interactable->Count += item->Interactable->Count;
			TraceLog(LOG_INFO, "ITEM PICK %d (stack) %d", i, ip->Interactable->Count);
			return true;
		}
	}

	return false;
}

void InventoryUse(InventoryContainer* i, ItemPickup* item)
{

}

void InventoryDraw(InventoryContainer* i)
{

}