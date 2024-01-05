#pragma once
#include <raylib.h>
#include <raymath.h>
#include "i_door.h"

typedef struct BlockCollider
{
	Vector2 Position;
	Vector2 Size;
} BlockCollider;

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

	int BlockColliderCount;
	BlockCollider* BlockColliders;

	int WallCount;
	Wall* Walls;

	int DoorCount;
	Door* Doors;
} MapData;

MapData* CurrentMapData;

void InitMap();
void UnloadMap();
void LoadMap(char* filename, MapData* map);
void DrawMap(MapData* map);

BlockCollider CreateBlockCollider(Vector2 pos, Vector2 size);

Wall CreateWall(Vector2 from, Vector2 to);
void UpdateWall(Wall* w);

Door CreateDoor(Vector2 pos, float rot, int id);