#pragma once
#include "asset_manager.h"

typedef struct Tile
{
	Vector2 Pos;
	Vector2 Rot;
	Vector2 Scale;
	char* TextureID[32];
	int SortIndex;
	Color Tint;

	Vector2 _meshPoints[4];
	Vector2 _uvPoints[4];
	TextureResource _textureResource;
} Tile;