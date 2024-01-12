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

	float Radius;
	InteractableType InteractableType;
	int InteractableSubType;

	bool IsActive;
	bool Activated;
	bool Hovered;
	int DataIndex;

	void (*OnInit)(struct Interactable* i);
	void (*OnUnload)(struct Interactable* i);
	void (*OnUpdate)(struct Interactable* i);
	void (*OnLateUpdate)(struct Interactable* i);
	void (*OnDraw)(struct Interactable* i);
	void (*OnInteract)(struct Interactable* i, PlayerCharacter* p);

} Interactable;

void InteractionInit();
void InteractionUnload();
void CheckInteraction();
void SetInteractableFunctions(Interactable* i);