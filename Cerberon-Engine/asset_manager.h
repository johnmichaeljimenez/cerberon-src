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

int TextureResourceCount;
TextureResource** TextureResourceList;

void LoadResources();
void UnloadResources();
void LoadTexturePack(char* filename, int* arrayCount, TextureResource** array);
void UnloadTexturePack(int* arrayCount, TextureResource** array);
TextureResource* GetTextureResource(char* name);