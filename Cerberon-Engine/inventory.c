#include "inventory.h"

bool InventoryAdd(InventoryContainer* ic, ItemPickup* item)
{
	ItemPickup** a = ic->Items;

	for (int i = 0; i < ic->MaxCount; i++)
	{
		ItemPickup* ip = a[i];
		if (ip == NULL || ip->ItemStatusType == ITEMSTATUSTYPE_None) //empty slot
		{
			item->IsActive = false;
			item->Interactable->IsActive = false;
			item->ItemStatusType = ITEMSTATUSTYPE_OnInventory;
			a[i] = item;
			item->CurrentSlotIndex = i;
			TraceLog(LOG_INFO, "ITEM PICK %d (empty) %d", i, item->CurrentAmount);
			return true;
		}

		if (item != NULL && ip->Interactable->InteractableSubType == item->Interactable->InteractableSubType && ip->CurrentAmount < ip->CurrentMaxAmount) //same type, should stack if allowed
		{
			ip->CurrentAmount += item->CurrentAmount;
			item->CurrentAmount = 0;
			if (ip->CurrentAmount > ip->CurrentMaxAmount)
			{
				int d = (ip->CurrentAmount - ip->CurrentMaxAmount);
				ip->CurrentAmount -= d;
				item->CurrentAmount += d;
			}

			TraceLog(LOG_INFO, "ITEM PICK %d (stack) %d/%d", i, ip->CurrentAmount, ip->CurrentMaxAmount);

			if (item->CurrentAmount <= 0)
			{
				ItemDestroy(item);
				return true;
			}
		}
	}

	return false;
}

void InventoryInit(InventoryContainer* in, int maxCount)
{
	for (int i = 0; i < InventoryMaxSize; i++)
	{
		in->Items[i] = NULL;
	}

	in->MaxCount = maxCount;
	in->CurrentSelectedIndex = -1;
}

void InventoryUnload(InventoryContainer* in)
{
	for (int i = 0; i < InventoryMaxSize; i++)
	{
		//in->Items[i]->Interactable = NULL;
		in->Items[i] = NULL;
	}
}

void InventoryUse(InventoryContainer* i, ItemPickup* item)
{
	if (item->OnUse != NULL)
		item->OnUse(item);
}

void InventoryDraw(InventoryContainer* i)
{

}

ItemPickup* InventoryGetItem(InventoryContainer* ic, InteractableSubType itemType)
{
	for (int i = 0; i < ic->MaxCount; i++)
	{
		if (ic->Items[i] != NULL && ic->Items[i]->Interactable != NULL && ic->Items[i]->Interactable->InteractableSubType == itemType)
			return ic->Items[i];
	}

	return NULL;
}