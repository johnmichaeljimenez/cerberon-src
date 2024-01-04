#pragma once
#include <raylib.h>
#include <raymath.h>

typedef struct Wall
{
	Vector2 From;
	Vector2 To;
	float Length;
	Vector2 Normal;
	Vector2 Midpoint;
} Wall;

typedef struct MapData
{
	Vector2 PlayerPosition;
	float PlayerRotation;

	int WallCount;
	Wall *Walls;
} MapData;

MapData* CurrentMapData;

void InitMap();
void UnloadMap();
void LoadMap(char* filename, MapData* map);
void DrawMap(MapData* map);
Wall CreateWall(Vector2 from, Vector2 to);
void UpdateWall(Wall* w);