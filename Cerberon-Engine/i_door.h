#pragma once
#include "interaction.h"

typedef struct Door
{
	bool IsActive;
	Interactable* Interactable;

	float Duration;

	bool IsOpen;
	float _timer;
} Door;

int DoorCount;

void LoadDoors();
void UnloadDoors();
void DoorInit(Interactable* d);
void DoorUnload(Interactable* d);
void DoorUpdate(Interactable* d);
void DoorLateUpdate(Interactable* d);
void DoorDraw(Interactable* d);

void DoorInteract(Door* d, PlayerCharacter* p);