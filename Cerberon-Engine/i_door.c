#include <raylib.h>
#include <raymath.h>
#include "memory.h"
#include "i_door.h"
#include "interaction.h"
#include "mapdata_manager.h"

void LoadDoors()
{
	DoorList = MCalloc(DoorCount, sizeof(Door), "Door List");

	int n = 0;
	for (int i = 0; i < CurrentMapData->InteractableCount; i++)
	{
		Interactable* in = &CurrentMapData->Interactables[i];
		if (in->InteractableType != INTERACTABLE_Door)
			continue;

		DoorList[n].DoorPosition = in->Position;
		DoorList[n].Interactable = in;
		DoorList[n].IsActive = true;
		DoorList[n].IsOpen = false;

		DoorList[n].Width = 32;
		DoorList[n].Length = 128;
		DoorList[n].Duration = 0.3;

		DoorList[n].From = Vector2Scale(Vector2Rotate((Vector2) { 1, 0 }, (in->Rotation - 90)* DEG2RAD), DoorList[n].Length / 2);
		DoorList[n].To = Vector2Scale(Vector2Rotate((Vector2) { 1, 0 }, (in->Rotation + 90)* DEG2RAD), DoorList[n].Length / 2);

		DoorList[n].From = Vector2Add(DoorList[n].From, DoorList[n].DoorPosition);
		DoorList[n].To = Vector2Add(DoorList[n].To, DoorList[n].DoorPosition);

		DoorList[n]._timer = 0;

		in->DataIndex = n;
		n++;
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