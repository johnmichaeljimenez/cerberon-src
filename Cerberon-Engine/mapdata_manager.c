#pragma warning(disable:4996)
#include <raylib.h>
#include <raymath.h>
#include <stdio.h>
#include <stdlib.h>
#include "mapdata_manager.h"

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
	 * wall block count
	 * - wall block 1
	 * -- position x
	 * -- position y
	 * -- size x
	 * -- size y
	 *
	 * - wall block 2
	 * -- ...
	 *
	 */

	FILE* file = fopen(filename, "rb");

	float x, y, r, w, h, n;

	fread(&x, sizeof(float), 1, file);
	fread(&y, sizeof(float), 1, file);
	fread(&r, sizeof(float), 1, file);

	map->PlayerPosition = (Vector2){ x, y };
	map->PlayerRotation = r;

	fread(&n, sizeof(int), 1, file);
	map->WallCount = n;
	map->Walls = calloc(n * 4, sizeof(Wall));

	for (int i = 0; i < n; i++)
	{
		fread(&x, sizeof(float), 1, file);
		fread(&y, sizeof(float), 1, file);
		fread(&w, sizeof(float), 1, file);
		fread(&h, sizeof(float), 1, file);

		//map->Walls[i].From = (Vector2){ x, y };
		//map->Walls[i].To = (Vector2){ x, y };
	}

	fclose(file);
}