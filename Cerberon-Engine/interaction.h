#pragma once
#include <raylib.h>
#include "player.h"

typedef enum InteractableType
{
	INTERACTABLE_Door,
	INTERACTABLE_Item,
} InteractableType;

typedef struct Interactable
{
	Vector2 Position;
	float Rotation;
	char* Target[32];
	char* TargetName[32];
	int Flags;

	float Radius;
	InteractableType InteractableType;

	bool IsActive;
	bool Activated;

	void (*OnInit)(struct Interactable* i);
	void (*OnUnload)(struct Interactable* i);
	void (*OnUpdate)(struct Interactable* i);
	void (*OnLateUpdate)(struct Interactable* i);
	void (*OnDraw)(struct Interactable* i);
	void (*OnInteract)(struct Interactable* i, PlayerCharacter* p);

} Interactable;

void SetInteractableFunctions(Interactable* i);