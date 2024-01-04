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
Door* DoorCreate(Door* d);
void DoorInit(Door* d);
void DoorUnload(Door* d);
void DoorUpdate(Door* d);
void DoorDraw(Door* d);

void DoorInteract(Door* d, PlayerCharacter* p);