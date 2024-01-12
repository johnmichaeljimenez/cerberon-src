#include "inventory.h"

bool InventoryAdd(InventoryContainer* ic, ItemPickup* item)
{
	ItemPickup** a = ic->Items;

	for (int i = 0; i < InventoryMaxSize; i++)
	{
		ItemPickup* ip = a[i];
		if (ip == NULL || ip->ItemStatusType == ITEMSTATUSTYPE_None) //empty slot
		{
			item->IsActive = false;
			item->Interactable->IsActive = false;
			item->ItemStatusType = ITEMSTATUSTYPE_OnInventory;
			a[i] = item;
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

			if (item->CurrentAmount <= 0)
				ItemDestroy(item);

			TraceLog(LOG_INFO, "ITEM PICK %d (stack) %d/%d", i, ip->CurrentAmount, ip->CurrentMaxAmount);
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