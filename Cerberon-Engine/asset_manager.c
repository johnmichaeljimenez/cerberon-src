#pragma warning(disable:4996)
#include<raylib.h>
#include "asset_manager.h"
#include <string.h>
#include <stdio.h>
#include <stdlib.h>

void LoadResources()
{
	TextureResourceList = calloc(1, sizeof(TextureResource));

	LoadTexturePack("res/tiles.pak", &TextureResourceCount, &TextureResourceList, TEXTURETYPE_Tile);
	LoadTexturePack("res/sprites.pak", &TextureResourceCount, &TextureResourceList, TEXTURETYPE_Sprite);
}

void UnloadResources()
{
	UnloadTexturePack(&TextureResourceCount, &TextureResourceList);
}

void LoadTexturePack(char* filename, int* arrayCount, TextureResource** texArray, TextureType type)
{
	FILE* file = fopen(filename, "rb");

	int c = 0;
	int start;
	fread(&c, sizeof(int), 1, file);

	start = *arrayCount;
	*arrayCount += c;
	*texArray = realloc(*texArray, sizeof(TextureResource) * *arrayCount);

	for (int i = start; i < start+c; i++)
	{
		char* texName[32];
		fread(texName, sizeof(char), 32, file);
		int texSize;
		fread(&texSize, sizeof(int), 1, file);
		char* texData = calloc(texSize, sizeof(char));
		fread(texData, sizeof(char), texSize, file);
		Image img = LoadImageFromMemory(".png", texData, texSize);

		strcpy((*texArray)[i].Name, texName);
		(*texArray)[i].Texture = LoadTextureFromImage(img);
		(*texArray)[i].TextureType = type;

		free(texData);
		UnloadImage(img);

		TraceLog(LOG_INFO, "Set texture index %d/%d %s", i, *arrayCount, texName);
	};

	fclose(file);
}

void UnloadTexturePack(int* arrayCount, TextureResource** texArray)
{
	for (int i = 0; i < *arrayCount; i++)
	{
		UnloadTexture((*texArray)[i].Texture);
	}

	free(*texArray);
}

TextureResource* GetTextureResource(char* name)
{
	int* arrayCount = &TextureResourceCount;
	TextureResource** texArray = &TextureResourceList;

	for (int i = 0; i < *arrayCount; i++)
	{
		if (strcmp((*texArray)[i].Name, name) == 0)
		{
			return &(*texArray)[i];
		}
	}

	return NULL;
}