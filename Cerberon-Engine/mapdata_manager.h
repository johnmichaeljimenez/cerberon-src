#pragma once
#include <raylib.h>
#include <raymath.h>
#include "lighting.h"
#include "i_door.h"

typedef struct BlockCollider
{
	Vector2 Position;
	Vector2 Size;
} BlockCollider;

typedef enum WallFlag
{
	WALLFLAG_NONE = 1 << 0,
	WALLFLAG_CAST_SHADOW = 1 << 1,
	WALLFLAG_IGNORE_RAYCAST = 1 << 2,
} WallFlag;

typedef struct Wall
{
	Vector2 From;
	Vector2 To;
	float Length;
	Vector2 Normal;
	Vector2 Midpoint;
	WallFlag WallFlags;

	Vector2 sFrom, sTo, sFrom2, sTo2;
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

	int LightCount;
	Light* Lights;
} MapData;

MapData* CurrentMapData;

void InitMap();
void UnloadMap();
void LoadMap(char* filename, MapData* map);
void UpdateMap(MapData* map);
void DrawMap(MapData* map);
void DrawMapHUD(MapData* map);

BlockCollider CreateBlockCollider(Vector2 pos, Vector2 size);

Wall CreateWall(Vector2 from, Vector2 to);
void UpdateWall(Wall* w);

Door CreateDoor(Vector2 pos, float rot, int id);