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
	fread(&c, sizeof(int), 1, file);

	*arrayCount += c;
	*texArray = malloc(sizeof(TextureResource) * *arrayCount);

	for (int i = 0; i < *arrayCount; i++)
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

		free(texData);
		UnloadImage(img);
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
	for (int i = 0; i < TextureResourceCount; i++)
	{
		if (strcmp(name, TextureResourceList[i]->Name) == 0)
		{
			return &TextureResourceList[i];
		}
	}

	return NULL;
}