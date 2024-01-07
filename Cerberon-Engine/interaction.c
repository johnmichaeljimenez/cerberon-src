#include <raylib.h>
#include "interaction.h"
#include "player.h"
#include "i_door.h"

void SetInteractableFunctions(Interactable* i)
{
	switch (i->InteractableType)
	{
	case INTERACTABLE_Door:
		i->OnInit = DoorInit;
		i->OnUpdate = DoorUpdate;
		i->OnLateUpdate = DoorLateUpdate;
		i->OnDraw = DoorDraw;
		i->OnUnload = DoorUnload;

		break;
	}
}