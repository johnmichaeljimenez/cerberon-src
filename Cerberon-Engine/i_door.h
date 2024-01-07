#pragma once
#include "interaction.h"

typedef struct Door
{
	Vector2 DoorPosition;
	bool IsActive;
	Interactable* Interactable;

	float Duration;
	float Width;
	float Length;

	bool IsOpen;
	float _timer;
	Vector2 From;
	Vector2 To;
} Door;

int DoorCount;
Door* DoorList;

void LoadDoors();
void UnloadDoors();
void DoorInit(Interactable* i);
void DoorUnload(Interactable* i);
void DoorUpdate(Interactable* i);
void DoorLateUpdate(Interactable* i);
void DoorDraw(Interactable* i);

void DoorInteract(Interactable* i, PlayerCharacter* p);