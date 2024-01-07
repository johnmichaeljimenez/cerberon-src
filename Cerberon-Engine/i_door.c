#include <raylib.h>
#include <raymath.h>
#include "memory.h"
#include "i_door.h"
#include "interaction.h"
#include "mapdata_manager.h"

void LoadDoors()
{
	DoorList = MCalloc(DoorCount, sizeof(Door), "Door List");

	for (int i = 0; i < DoorCount; i++)
	{
		Interactable* in = &CurrentMapData->Interactables[i];
		if (in->InteractableType != INTERACTABLE_Door)
			continue;

		DoorList[i].DoorPosition = in->Position;
		DoorList[i].Interactable = in;
		DoorList[i].IsActive = true;
		DoorList[i].IsOpen = false;

		DoorList[i].Width = 32;
		DoorList[i].Length = 128;
		DoorList[i].Duration = 0.3;

		DoorList[i].From = Vector2Scale(Vector2Rotate((Vector2) { 1, 0 }, (in->Rotation - 90)* DEG2RAD), DoorList[i].Length / 2);
		DoorList[i].To = Vector2Scale(Vector2Rotate((Vector2) { 1, 0 }, (in->Rotation + 90)* DEG2RAD), DoorList[i].Length / 2);

		DoorList[i].From = Vector2Add(DoorList[i].From, DoorList[i].DoorPosition);
		DoorList[i].To = Vector2Add(DoorList[i].To, DoorList[i].DoorPosition);

		DoorList[i]._timer = 0;

		in->DataIndex = i;
	}
}

void UnloadDoors()
{
	MFree(DoorList, DoorCount, sizeof(Door), "Door List");
}

void DoorInit(Interactable* i)
{

}

void DoorUnload(Interactable* i)
{

}

void DoorUpdate(Interactable* i)
{
	Door* d = &DoorList[i->DataIndex];
	if (d->_timer <= 0)
		return;

	d->_timer -= GetFrameTime();
	if (d->_timer <= 0)
		d->_timer = 0;

	Vector2 openPos = Vector2Scale(Vector2Rotate((Vector2) { 1, 0 }, (i->Rotation + 90)* DEG2RAD), d->Length / 1.25);
	openPos = Vector2Add(i->Position, openPos);

	Vector2 from = d->IsOpen ? openPos : i->Position;
	Vector2 to = d->IsOpen ? i->Position : openPos;

	d->DoorPosition = Vector2Lerp(from, to, d->_timer / d->Duration);

	d->From = Vector2Scale(Vector2Rotate((Vector2) { 1, 0 }, (i->Rotation - 90)* DEG2RAD), d->Length / 2);
	d->To = Vector2Scale(Vector2Rotate((Vector2) { 1, 0 }, (i->Rotation + 90)* DEG2RAD), d->Length / 2);

	d->From = Vector2Add(d->From, d->DoorPosition);
	d->To = Vector2Add(d->To, d->DoorPosition);
}

void DoorLateUpdate(Interactable* i)
{

}

void DoorDraw(Interactable* i)
{
	Door* d = &DoorList[i->DataIndex];

	//Temporary (use sprite later)
	DrawLineEx(d->From, d->To, d->Width, BLACK);

	if (i->Hovered)
		DrawCircleV(i->Position, 4, WHITE);
}

void DoorInteract(Interactable* i, PlayerCharacter* p)
{
	Door* d = &DoorList[i->DataIndex];
	d->_timer = d->Duration;
	d->IsOpen = !d->IsOpen;
}