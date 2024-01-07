#pragma once
#include "interaction.h"

typedef struct Door
{
	Vector2 Position;
	float Rotation;

	float Duration;

	bool IsOpen;
	float _timer;
} Door;


void LoadDoors();
void UnloadDoors();
void DoorInit(Interactable* d);
void DoorUnload(Interactable* d);
void DoorUpdate(Interactable* d);
void DoorLateUpdate(Interactable* d);
void DoorDraw(Interactable* d);

void DoorInteract(Door* d, PlayerCharacter* p);