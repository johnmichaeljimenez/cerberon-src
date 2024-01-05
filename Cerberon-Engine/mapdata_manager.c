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
	if (CurrentMapData->BlockColliderCount > 0)
		MFree(CurrentMapData->BlockColliders, CurrentMapData->BlockColliderCount, sizeof(BlockCollider), "BC List");

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
	 * -- center x
	 * -- center y
	 * -- width
	 * -- height
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
	map->BlockColliderCount = n;
	map->WallCount = n * 4;

	if (map->WallCount > 0)
	{
		map->BlockColliders = MCalloc(map->BlockColliderCount, sizeof(Wall), "BC List");
		map->Walls = MCalloc(map->WallCount, sizeof(Wall), "Wall List");

		for (int i = 0; i < map->BlockColliderCount; i++)
		{
			fread(&x1, sizeof(float), 1, file);
			fread(&y1, sizeof(float), 1, file);
			fread(&x2, sizeof(float), 1, file);
			fread(&y2, sizeof(float), 1, file);

			map->BlockColliders[i] = CreateBlockCollider((Vector2) { x1, y1 }, (Vector2) { x2, y2 });

		}

		for (int i = 0; i < map->WallCount; i += 4)
		{
			BlockCollider* block = &map->BlockColliders[i / 4];
			x1 = block->Position.x;
			y1 = block->Position.y;
			x2 = block->Size.x;
			y2 = block->Size.y;

			x2 /= 2;
			y2 /= 2;

			Vector2 a = (Vector2){ x1 - x2, y1 - y2 }; //upper left
			Vector2 b = (Vector2){ x1 - x2, y1 + y2 }; //lower left
			Vector2 c = (Vector2){ x1 + x2, y1 + y2 }; //lower right
			Vector2 d = (Vector2){ x1 + x2, y1 - y2 }; //upper right

			map->Walls[i] = CreateWall(a, b);
			map->Walls[i + 1] = CreateWall(b, c);
			map->Walls[i + 2] = CreateWall(c, d);
			map->Walls[i + 3] = CreateWall(d, a);
		}
	}


	//fread(&n, sizeof(int), 1, file);
	//map->DoorCount = n;

	//if (map->DoorCount > 0)
	//{
	//	map->Doors = MCalloc(n, sizeof(Door), "Door List");

	//	for (int i = 0; i < n; i++)
	//	{
	//		fread(&x1, sizeof(float), 1, file);
	//		fread(&y1, sizeof(float), 1, file);
	//		fread(&r, sizeof(float), 1, file);
	//		fread(&id, sizeof(int), 1, file);

	//		map->Doors[i] = CreateDoor((Vector2) { x1, y1 }, r, id);
	//	}
	//}

	fclose(file);
}

BlockCollider CreateBlockCollider(Vector2 pos, Vector2 size)
{
	BlockCollider bc = { 0 };

	bc.Position = pos;
	bc.Size = size;

	return bc;
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