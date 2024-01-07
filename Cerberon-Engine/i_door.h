#pragma once
#include "interaction.h"

typedef struct Door
{
	Vector2 DoorPosition;
	bool IsActive;
	Interactable* Interactable;

	float Duration;

	bool IsOpen;
	float _timer;
} Door;

int DoorCount;

void LoadDoors();
void UnloadDoors();
void DoorInit(Interactable* i);
void DoorUnload(Interactable* i);
void DoorUpdate(Interactable* i);
void DoorLateUpdate(Interactable* i);
void DoorDraw(Interactable* i);

void DoorInteract(Interactable* i, PlayerCharacter* p);