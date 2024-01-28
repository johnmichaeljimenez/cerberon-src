#pragma once
#include <raylib.h>
#include <raymath.h>
#include "player.h"
#include "interaction.h"

typedef enum ItemStatusType
{
	ITEMSTATUSTYPE_None,
	ITEMSTATUSTYPE_OnWorld,
	ITEMSTATUSTYPE_OnInventory,
} ItemStatusType;

typedef struct ItemPickup
{
	bool IsActive;
	Interactable* Interactable;
	int Index;

	ItemStatusType ItemStatusType;
	int CurrentSlotIndex;

	int CurrentAmount;
	int CurrentMaxAmount;
	bool(*OnPickup)(struct ItemPickup* item);
	bool(*OnUse)(struct ItemPickup* item);
} ItemPickup;

int NextItemSlotIndex;
int ItemCount;
ItemPickup* ItemList;

void LoadItems();
void UnloadItems();
void ItemInit(Interactable* i);
void ItemUnload(Interactable* i);
void ItemUpdate(Interactable* i);
void ItemLateUpdate(Interactable* i);
void ItemDraw(Interactable* i);

bool ItemInteract(Interactable* i, PlayerCharacter* p);

void ItemDestroy(ItemPickup* i);
bool Pickup(ItemPickup* i);
bool OnMedkitUse(ItemPickup* i);
bool OnFlashlightUse(ItemPickup* i);
bool OnWeaponPickup(ItemPickup* i);