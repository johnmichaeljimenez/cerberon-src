#pragma once
#include <raylib.h>
#include <raymath.h>
#include "lighting.h"
#include "interaction.h"
#include "tile.h"

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

	Rectangle _Bounds;
} Wall;

typedef struct MapData
{
	Vector2 PlayerPosition;
	float PlayerRotation;

	int BlockColliderCount;
	BlockCollider* BlockColliders;

	int WallCount;
	Wall* Walls;

	int InteractableCount;
	Interactable* Interactables;

	int LightCount;
	Light* Lights;

	int TileCount;
	Tile* Tiles;
} MapData;

MapData* CurrentMapData;

void InitMap();
void UnloadMap();
void LoadMap(char* filename, MapData* map);
void UpdateMap(MapData* map);
void DrawMap(MapData* map);
void DrawMapHUD(MapData* map);

BlockCollider CreateBlockCollider(Vector2 pos, Vector2 size);

Wall CreateWall(Vector2 from, Vector2 to, WallFlag flags);
void UpdateWall(Wall* w);
void DrawWalls();

Interactable CreateInteractable(Vector2 pos, float rot, char* target, char* targetname, InteractableType intType, InteractableSubType intSubType, int flags, int count);
