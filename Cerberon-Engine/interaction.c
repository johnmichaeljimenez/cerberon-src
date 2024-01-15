#include <raylib.h>
#include "interaction.h"
#include "player.h"
#include "mapdata_manager.h"
#include "camera.h"
#include "collision.h"
#include "cursor.h"
#include "i_door.h"
#include "i_item.h"
#include "time.h"
#include "input_handler.h"
#include <string.h>

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

	case INTERACTABLE_Item:
		i->Radius = 16;
		i->OnInit = ItemInit;
		i->OnUpdate = ItemUpdate;
		i->OnLateUpdate = ItemLateUpdate;
		i->OnDraw = ItemDraw;
		i->OnUnload = ItemUnload;
		i->OnInteract = ItemInteract;
	}
}

void InteractionInit()
{
	LoadDoors();
	LoadItems();
}

void CheckInteraction()
{
	//TODO: Sort by nearest valid objects first before interaction
	for (int i = 0; i < CurrentMapData->InteractableCount; i++)
	{
		Interactable* a = &CurrentMapData->Interactables[i];
		a->Hovered = false;

		if (a->OneShot && a->Activated)
			continue;

		if (!a->IsActive)
			continue;

		if (!a->ChainActivated)
		{
			if (Vector2DistanceSqr(a->Position, PlayerEntity.Position) > (PlayerEntity.InteractionRadius * PlayerEntity.InteractionRadius))
				continue;

			a->Hovered = CheckCollisionCircles(a->Position, a->Radius, CameraGetMousePosition(), 64);

			if (a->Hovered)
			{
				CursorChange(CURSORSTATE_IngameInteractHover);
				LinecastHit hit;
				Linecast(PlayerEntity.Position, a->Position, &hit);
				if (hit.Hit && Vector2DistanceSqr(hit.To, a->Position) > 16)
				{
					a->Hovered = false;
					continue;
				}
			}

			if (!a->Hovered)
				continue;

			Vector2 curPos = a->Position;
			curPos = Vector2Lerp(GetMousePosition(), GetWorldToScreen2D(a->Position, GameCamera), 0.6f);
			CursorOverridePosition(curPos);
			CursorChange(CURSORSTATE_IngameInteractEnabled);

			bool pressed = InputGetPressed(INPUTACTIONTYPE_Interact);
			if (!pressed)
				continue;
		}

		if (a->OnInteract(a, &PlayerEntity))
		{
			a->ChainActivated = false;
			a->Activated = true;

			Interactable* target = FindInteractable(a->Target);
			if (target != 0)
			{
				target->ChainActivated = true;
			}

			break;
		}
	}
}

void InteractionUnload()
{
	UnloadItems();
	UnloadDoors();
}

Interactable* FindInteractable(char* targetName)
{
	for (int i = 0; i < CurrentMapData->InteractableCount; i++)
	{
		Interactable* in = &CurrentMapData->Interactables[i];
		if (strcmp(in->TargetName, targetName) == 0)
			return in;
	}

	return NULL;
}