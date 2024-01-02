#pragma once
#include <raylib.h>

typedef enum TextureType
{
	TEXTURETYPE_Sprite,
	TEXTURETYPE_Tile,
} TextureType;

typedef struct TextureResource
{
	char* Name[32];
	Texture2D Texture;
	TextureType TextureType;
} TextureResource;

void LoadTexturePack(char* filename, int* arrayCount, TextureResource** array);
void UnloadTexturePack(int* arrayCount, TextureResource** array);