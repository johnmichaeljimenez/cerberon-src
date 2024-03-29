#pragma once
#include <raylib.h>
#include "player.h"

typedef enum InteractableType
{
	INTERACTABLE_Door,
	INTERACTABLE_Item,
} InteractableType;

typedef enum InteractableSubType
{
	INTERACTABLESUB_Door,
	INTERACTABLESUB_ItemMedkit,
	INTERACTABLESUB_ItemFlashlight,
	INTERACTABLESUB_ItemCash,
	INTERACTABLESUB_ItemWeaponPistol,
	INTERACTABLESUB_ItemBackpack,
} InteractableSubType;

typedef struct Interactable
{
	Vector2 Position;
	float Rotation;
	char* Target[32];
	char* TargetName[32];
	int Flags;
	bool OneShot;
	int Count;
	float Delay;

	char ParamID[16];
	float ParamFloat;
	int ParamInt;
	bool ParamBool;

	float Radius;
	InteractableType InteractableType;
	int InteractableSubType;

	bool IsActive;
	bool Activated;
	bool ChainActivated;
	bool Hovered;
	int DataIndex;
	float DelayTimer;

	void (*OnInit)(struct Interactable* i);
	void (*OnUnload)(struct Interactable* i);
	void (*OnUpdate)(struct Interactable* i);
	void (*OnLateUpdate)(struct Interactable* i);
	void (*OnDraw)(struct Interactable* i);
	bool (*OnInteract)(struct Interactable* i, CharacterEntity* p);

} Interactable;

void InteractionInit();
void InteractionUnload();
void CheckInteraction();
void SetInteractableFunctions(Interactable* i);
Interactable* FindInteractable(char* targetName);