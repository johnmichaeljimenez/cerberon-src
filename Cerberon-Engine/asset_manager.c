#pragma warning(disable:4996)
#include <raylib.h>
#include "asset_manager.h"
#include <string.h>
#include <stdio.h>
#include "utils.h"
#include "memory.h"

//TODO: investigate memory leakage from logfile report
void LoadResources()
{
	TextureResourceList = MCalloc(2, sizeof(TextureResource), "Texture List");
	TextureResourceCount = 2;

	Image img = GenImageChecked(128, 128, 16, 16, MAGENTA, BLACK);
	TextureResourceList[0] = (TextureResource){
		.Name = "NULL",
		.Hash = 0,
		.Texture = LoadTextureFromImage(img),
		.TextureType = TEXTURETYPE_Tile
	};
	UnloadImage(img);

	img = GenImageGradientRadial(128, 128, 0, WHITE, BLACK);
	TextureResourceList[1] = (TextureResource){
		.Name = "misc-light",
		.Hash = ToHash("misc-light"),
		.Texture = LoadTextureFromImage(img),
		.TextureType = TEXTURETYPE_Sprite
	};
	UnloadImage(img);

	LoadTexturePack("res/tiles.pak", &TextureResourceCount, &TextureResourceList, TEXTURETYPE_Tile);
	LoadTexturePack("res/sprites.pak", &TextureResourceCount, &TextureResourceList, TEXTURETYPE_Sprite);

	WallTexture = GetTextureResource(ToHash("misc-wall"));
	WallNPatch = (NPatchInfo){ (Rectangle) { 0.0f, 0.0f, WallTexture->Texture.width, WallTexture->Texture.height }, 24, 24, 24, 24, NPATCH_NINE_PATCH };
}

void UnloadResources()
{
	UnloadTexturePack(&TextureResourceCount, &TextureResourceList);
}

void LoadTexturePack(char* filename, int* arrayCount, TextureResource** texArray, TextureType type)
{
	/*
	* FORMAT:
	* texture count
	*
	* - texture 1
	* -- name [32]
	* -- tex data length
	* -- tex data
	*
	* - texture 2
	* -- ...
	*/

	FILE* file = fopen(filename, "rb");

	int c = 0;
	size_t start;
	fread(&c, sizeof(int), 1, file);

	start = *arrayCount;

	*arrayCount += c;
	*texArray = MRealloc(*texArray, *arrayCount, sizeof(TextureResource), start, "Texture List");

	for (int i = start; i < start + c; i++)
	{
		char* texName[32];
		fread(texName, sizeof(char), 32, file);
		int texSize;
		fread(&texSize, sizeof(int), 1, file);
		char* texData = MCalloc(texSize, sizeof(char), "Temp image data");
		fread(texData, sizeof(char), texSize, file);
		Image img = LoadImageFromMemory(".png", texData, texSize);

		strcpy((*texArray)[i].Name, texName);
		(*texArray)[i].Texture = LoadTextureFromImage(img);
		(*texArray)[i].TextureType = type;
		(*texArray)[i].Hash = ToHash(texName);

		MFree(texData, texSize, sizeof(char), "Temp image data");
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

	MFree(*texArray, *arrayCount, sizeof(TextureResource), "Texture List");
}

TextureResource* GetTextureResource(unsigned long hash)
{
	int* arrayCount = &TextureResourceCount;
	TextureResource** texArray = &TextureResourceList;

	for (int i = 0; i < *arrayCount; i++)
	{
		if ((*texArray)[i].Hash == hash)
			return &(*texArray)[i];
	}

	return NULL;
}


void LoadAnimationPack(char* filename, int* arrayCount, AnimationClip** clipArray)
{
	/*
	* FORMAT:
	* clip count
	*
	* - clip 1
	* -- name [32]
	* -- clip frame length
	* -- clip frames
	*
	* - clip 2
	* -- ...
	*/


	FILE* file = fopen(filename, "rb");

	int c = 0;
	int start;
	fread(&c, sizeof(int), 1, file);

	start = *arrayCount;
	*arrayCount += c;
	*clipArray = realloc(*clipArray, sizeof(AnimationClip) * *arrayCount);

	for (int i = start; i < start + c; i++)
	{
		//char* clipName[32];
		//fread(texName, sizeof(char), 32, file);
		//int texSize;
		//fread(&texSize, sizeof(int), 1, file);
		//char* texData = calloc(texSize, sizeof(char));
		//fread(texData, sizeof(char), texSize, file);

		//strcpy((*texArray)[i].Name, texName);
		//(*texArray)[i].Texture = LoadTextureFromImage(img);
		//(*texArray)[i].TextureType = type;
		//(*texArray)[i].Hash = ToHash(texName);

		//free(texData);
		//UnloadImage(img);

		//TraceLog(LOG_INFO, "Set animation clip index %d/%d %s", i, *arrayCount, texName);
	};

	fclose(file);
}

void UnloadAnimationPack(int* arrayCount, AnimationClip** array)
{

}

AnimationClip* GetAnimationResource(char* name)
{

}