#include <raylib.h>
#include <raymath.h>
#include "player.h"
#include "i_item.h"
#include "mapdata_manager.h"
#include "memory.h"
#include "inventory.h"
#include "renderer.h"
#include "utils.h"
#include "weapon_manager.h"

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
		ItemList[n].OnPickup = NULL;
		ItemList[n].CurrentSlotIndex = -1;

		if (in->InteractableSubType == INTERACTABLESUB_ItemMedkit)
		{
			ItemList[n].CurrentMaxAmount = 10;
			ItemList[n].OnUse = OnMedkitUse;
		}

		if (in->InteractableSubType == INTERACTABLESUB_ItemFlashlight)
		{
			ItemList[n].CurrentMaxAmount = 1;
			ItemList[n].OnUse = OnFlashlightUse;
		}

		if (in->InteractableSubType == INTERACTABLESUB_ItemWeaponPistol)
		{
			ItemList[n].CurrentMaxAmount = 1;
			ItemList[n].OnUse = NULL;
			ItemList[n].OnPickup = OnWeaponPickup;
		}

		if (in->InteractableSubType == INTERACTABLESUB_ItemBackpack)
		{
			ItemList[n].CurrentMaxAmount = 1;
			ItemList[n].OnUse = NULL;
			ItemList[n].OnPickup = OnBackpackPickup;
		}

		in->DataIndex = n;

		RenderObject* r = CreateRenderObject(RENDERLAYER_Entity, 0, (Rectangle) { 0, 0, 0, 0 }, (void*)in, ItemDraw, NULL);
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

	//DrawCircleV(i->Position, 32, BLUE);
	SetShaderMaskedMode();
	TextureResource* t = GetTextureResource(ToHash("survivor-idle_0"));
	DrawSprite(t, i->Position, 0, 1, Vector2Zero(), WHITE);
	EndShaderMode();
}

bool ItemInteract(Interactable* i, PlayerCharacter* p)
{
	ItemPickup* ip = &ItemList[i->DataIndex];
	return Pickup(ip);
}

void ItemDestroy(ItemPickup* i)
{
	i->Interactable->IsActive = false;
	i->IsActive = false;
	i->ItemStatusType = ITEMSTATUSTYPE_None;
}

bool Pickup(ItemPickup* i)
{
	bool picked = InventoryAdd(&InventoryPlayer, i);// i->OnUse(i);
	if (picked)
	{
		if (i->OnPickup != NULL)
			i->OnPickup(i);
	}

	return picked;
}

bool OnMedkitUse(ItemPickup* i)
{
	//heal
}

bool OnFlashlightUse(ItemPickup* i)
{
	//toggle flashlight
}

bool OnWeaponPickup(ItemPickup* i)
{
	PlayerAddWeapon(WeaponGive(WEAPONTYPE_Pistol, 12, 50), i);
}

bool OnBackpackPickup(ItemPickup* i)
{
	//PlayerAddWeapon(WeaponGive(WEAPONTYPE_Pistol, 12, 50), i);
}