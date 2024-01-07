#include <raylib.h>
#include "interaction.h"
#include "player.h"
#include "i_door.h"
#include "mapdata_manager.h"
#include "camera.h"

void SetInteractableFunctions(Interactable* i)
{
	switch (i->InteractableType)
	{
	case INTERACTABLE_Door:
		i->Radius = 32;
		i->OnInit = DoorInit;
		i->OnUpdate = DoorUpdate;
		i->OnLateUpdate = DoorLateUpdate;
		i->OnDraw = DoorDraw;
		i->OnUnload = DoorUnload;
		i->OnInteract = DoorInteract;

		break;
	}
}

void InteractionInit()
{
	LoadDoors();
}

void CheckInteraction()
{
	for (int i = 0; i < CurrentMapData->InteractableCount; i++)
	{
		Interactable* a = &CurrentMapData->Interactables[i];
		if (!a->IsActive)
			continue;

		a->Hovered = CheckCollisionCircles(a->Position, a->Radius, CameraGetMousePosition(), 32);
		if (!a->Hovered)
			continue;

		if (IsMouseButtonPressed(MOUSE_BUTTON_LEFT))
		{
			a->OnInteract(a, &PlayerEntity);
			break;
		}
	}
}

void InteractionUnload()
{
	UnloadDoors();
}