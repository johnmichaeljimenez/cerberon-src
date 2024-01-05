#pragma warning(disable:4996)
#include <raylib.h>
#include <raymath.h>
#include <stdio.h>
#include "mapdata_manager.h"
#include "memory.h"


void InitMap()
{
	CurrentMapData = MCalloc(1, sizeof(MapData), "Map Data");
	LoadMap("maps/test.map", CurrentMapData);
}

void UnloadMap()
{
	if (CurrentMapData->WallCount > 0)
		MFree(CurrentMapData->Walls, CurrentMapData->WallCount, sizeof(Wall), "Wall List");

	if (CurrentMapData->DoorCount > 0)
		MFree(CurrentMapData->Doors, CurrentMapData->DoorCount, sizeof(Door), "Door List");

	MFree(CurrentMapData, 1, sizeof(MapData), "Map Data");
}

void LoadMap(char* filename, MapData* map)
{
	/*
	 * FORMAT:
	 *
	 * map name [32]
	 *
	 * player pos x
	 * player pos y
	 * rotation
	 *
	 * wall count
	 * - wall 1
	 * -- from x
	 * -- from y
	 * -- to x
	 * -- to y
	 *
	 * - wall 2
	 * -- ...
	 * 
	 * door count
	 * - door 1
	 * -- pos x
	 * -- pos y
	 * -- rotation
	 * -- key id
	 * 
	 * - door 2
	 * -- ...
	 *
	 */

	FILE* file = fopen(filename, "rb");

	int n = 0, id = 0;
	float x1, y1, r, x2, y2;

	fread(&x1, sizeof(float), 1, file);
	fread(&y1, sizeof(float), 1, file);
	fread(&r, sizeof(float), 1, file);

	map->PlayerPosition = (Vector2){ x1, y1 };
	map->PlayerRotation = r;

	fread(&n, sizeof(int), 1, file);
	map->WallCount = n * 4;

	if (map->WallCount > 0)
	{
		map->Walls = MCalloc(n * 4, sizeof(Wall), "Wall List");

		for (int i = 0; i < n; i++)
		{
			fread(&x1, sizeof(float), 1, file);
			fread(&y1, sizeof(float), 1, file);
			fread(&x2, sizeof(float), 1, file);
			fread(&y2, sizeof(float), 1, file);

			map->Walls[i] = CreateWall((Vector2) { x1, y1 }, (Vector2) { x2, y2 });
		}
	}


	fread(&n, sizeof(int), 1, file);
	map->DoorCount = n;

	if (map->DoorCount > 0)
	{
		map->Doors = MCalloc(n, sizeof(Door), "Door List");

		for (int i = 0; i < n; i++)
		{
			fread(&x1, sizeof(float), 1, file);
			fread(&y1, sizeof(float), 1, file);
			fread(&r, sizeof(float), 1, file);
			fread(&id, sizeof(int), 1, file);

			map->Doors[i] = CreateDoor((Vector2) { x1, y1 }, r, id);
		}
	}

	fclose(file);
}

Wall CreateWall(Vector2 from, Vector2 to)
{
	Wall w = { 0 };
	w.From = from;
	w.To = to;

	UpdateWall(&w);

	return w;
}

Door CreateDoor(Vector2 pos, float rot, int id)
{
	Door d = { 0 };

	return d;
}

void DrawMap(MapData* map)
{
	for (int i = 0; i < map->WallCount; i++)
	{
		Wall w = map->Walls[i];

		DrawLineV(w.From, w.To, WHITE);
	}
}

void UpdateWall(Wall* w)
{
	Vector2 diff = Vector2Subtract(w->To, w->From);
	w->Normal = Vector2Normalize((Vector2) { -diff.y, diff.x });
	w->Length = Vector2Length(diff);
	w->Midpoint = Vector2Add(w->From, w->To);
	w->Midpoint.x /= 2;
	w->Midpoint.y /= 2;
}