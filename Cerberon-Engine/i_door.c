#include <raylib.h>
#include <raymath.h>
#include "memory.h"
#include "i_door.h"
#include "interaction.h"
#include "mapdata_manager.h"

static Door* DoorList;

void LoadDoors()
{
	DoorList = MCalloc(DoorCount, sizeof(Door), "Door List");

	for (int i = 0; i < DoorCount; i++)
	{
		Interactable* in = &CurrentMapData->Interactables[i];
		if (in->InteractableType != INTERACTABLE_Door)
			continue;

		DoorList[i].Interactable = in;
		DoorList[i].IsActive = true;
		DoorList[i].IsOpen = false;

		DoorList[i].Duration = 1;
		DoorList[i]._timer = 0;

		in->DataIndex = i;
	}
}

void UnloadDoors()
{
	MFree(DoorList, DoorCount, sizeof(Door), "Door List");
}

void DoorInit(Interactable* d)
{

}

void DoorUnload(Interactable* d)
{

}

void DoorUpdate(Interactable* d)
{

}

void DoorLateUpdate(Interactable* d)
{

}

void DoorDraw(Interactable* d)
{
	DrawCircleV(d->Position, 64, WHITE);
}

void DoorInteract(Door* d, PlayerCharacter* p)
{

}