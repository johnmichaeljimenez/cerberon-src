#pragma once
#include <raylib.h>
#include <raymath.h>
#include "lighting.h"
#include "interaction.h"
#include "tile.h"
#include "i_trigger.h"
#include "overlay.h"

typedef enum WallHeight
{
	WALLHEIGHT_Default = 0,
	WALLHEIGHT_Low = 1,
	WALLHEIGHT_Crawlspace = 2
} WallHeight;

typedef struct BlockCollider
{
	Vector2 Position;
	Vector2 Size;
	bool IsCircle;
	WallHeight WallHeight;
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
	WallHeight WallHeight;
	bool IsCircle;
	Vector2 CirclePosition;
	float CircleRadius;

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

	int TriggerCount;
	Trigger* Triggers;

	int OverlayCount;
	Overlay* Overlays;
} MapData;

MapData* CurrentMapData;

void InitMap();
void UnloadMap();
void LoadMap(char* filename, MapData* map);
void UpdateMap(MapData* map);
void DrawMap(MapData* map);
void DrawMapHUD(MapData* map);

Wall CreateWall(Vector2 from, Vector2 to, WallFlag flags);
void UpdateWall(Wall* w);
void DrawWalls();
void DrawWallBlock(BlockCollider* b);