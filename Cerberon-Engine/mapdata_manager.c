#pragma warning(disable:4996)
#include <raylib.h>
#include <raymath.h>
#include <stdio.h>
#include <stdlib.h>
#include "mapdata_manager.h"


void InitMap()
{
	CurrentMapData = calloc(1, sizeof(MapData));
	LoadMap("maps/test.map", CurrentMapData);
}

void UnloadMap()
{
	free(CurrentMapData->Walls);
	free(CurrentMapData);
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
	 */

	FILE* file = fopen(filename, "rb");

	int n = 0;
	float x1, y1, r, x2, y2;

	fread(&x1, sizeof(float), 1, file);
	fread(&y1, sizeof(float), 1, file);
	fread(&r, sizeof(float), 1, file);

	map->PlayerPosition = (Vector2){ x1, y1 };
	map->PlayerRotation = r;

	fread(&n, sizeof(int), 1, file);
	map->WallCount = n;
	map->Walls = calloc(n * 4, sizeof(Wall));

	for (int i = 0; i < n; i++)
	{
		fread(&x1, sizeof(float), 1, file);
		fread(&y1, sizeof(float), 1, file);
		fread(&x2, sizeof(float), 1, file);
		fread(&y2, sizeof(float), 1, file);

		map->Walls[i] = CreateWall((Vector2) { x1, y1 }, (Vector2) { x2, y2 });
	}

	fclose(file);
}

Wall CreateWall(Vector2 from, Vector2 to)
{
	Wall w = {0};
	w.From = from;
	w.To = to;

	return w;
}

void DrawMap(MapData* map)
{
	for (int i = 0; i < map->WallCount; i++)
	{
		Wall w = map->Walls[i];

		DrawLineV(w.From, w.To, WHITE);
	}
}