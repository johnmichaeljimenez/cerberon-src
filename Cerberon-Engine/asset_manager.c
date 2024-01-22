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

	img = GenImageGradientRadial(512, 512, 0, WHITE, BLACK);
	TextureResourceList[1] = (TextureResource){
		.Name = "misc-light",
		.Hash = ToHash("misc-light"),
		.Texture = LoadTextureFromImage(img),
		.TextureType = TEXTURETYPE_Sprite
	};
	UnloadImage(img);

	LoadTexturePack("res/tiles.pak", TEXTURETYPE_Tile);
	LoadTexturePack("res/sprites.pak", TEXTURETYPE_Sprite);

	WallTexture = GetTextureResource(ToHash("misc-wall"));
	WallNPatch = (NPatchInfo){ (Rectangle) { 0.0f, 0.0f, WallTexture->Texture.width, WallTexture->Texture.height }, 36, 36, 36, 36, NPATCH_NINE_PATCH };

	LightTexture = GetTextureResource(ToHash("misc-light"));
	FlashlightTexture = GetTextureResource(ToHash("vfx-flashlight"));

	LoadAnimationPack("res/animations.pak");
}

void UnloadResources()
{
	UnloadAnimationPack();
	UnloadTexturePack(&TextureResourceCount, &TextureResourceList);
}

void LoadTexturePack(char* filename, TextureType type)
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

	start = TextureResourceCount;

	TextureResourceCount += c;
	TextureResourceList = MRealloc(TextureResourceList, TextureResourceCount, sizeof(TextureResource), start, "Texture List");

	for (int i = start; i < start + c; i++)
	{
		char* texName[32];
		fread(texName, sizeof(char), 32, file);
		int texSize;
		fread(&texSize, sizeof(int), 1, file);
		char* texData = MCalloc(texSize, sizeof(char), "Temp image data");
		fread(texData, sizeof(char), texSize, file);
		Image img = LoadImageFromMemory(".png", texData, texSize);

		strcpy_s(TextureResourceList[i].Name, 32, texName);
		TextureResourceList[i].Texture = LoadTextureFromImage(img);
		TextureResourceList[i].TextureType = type;
		TextureResourceList[i].Hash = ToHash(texName);

		MFree(texData, texSize, sizeof(char), "Temp image data");
		UnloadImage(img);

		TraceLog(LOG_INFO, "Set texture index %d/%d %s", i, TextureResourceCount, texName);
	};

	fclose(file);
}

void UnloadTexturePack()
{
	for (int i = 0; i < TextureResourceCount; i++)
	{
		UnloadTexture(TextureResourceList[i].Texture);
	}

	MFree(TextureResourceList, TextureResourceCount, sizeof(TextureResource), "Texture List");
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


void LoadAnimationPack(char* filename)
{
	//testing
	AnimationClipCount = 5;
	AnimationClipList = MCalloc(AnimationClipCount, sizeof(AnimationClip), "Animation Clip List");

	AnimationClipList[0] = (AnimationClip)
	{
		.FrameCount = 20,
		.Name = "player_idle",
		.Hash = ToHash("player_idle"),
		.Loop = true
	};

	for (int i = 0; i < 20; i++)
	{
		unsigned long hash = ToHash(TextFormat("survivor-idle_handgun_%d", i));
		AnimationClipList[0].Frames[i] = hash;
		AnimationClipList[0].SpriteFrames[i] = GetTextureResource(hash);
	}

	AnimationClipList[1] = (AnimationClip)
	{
		.FrameCount = 20,
		.Name = "player_move",
		.Hash = ToHash("player_move"),
		.Loop = true
	};

	for (int i = 0; i < 20; i++)
	{
		unsigned long hash = ToHash(TextFormat("survivor-move_handgun_%d", i));
		AnimationClipList[1].Frames[i] = hash;
		AnimationClipList[1].SpriteFrames[i] = GetTextureResource(hash);
	}


	AnimationClipList[2] = (AnimationClip)
	{
		.FrameCount = 15,
		.Name = "player_attack",
		.Hash = ToHash("player_attack"),
		.Loop = false
	};

	for (int i = 0; i < 15; i++)
	{
		unsigned long hash = ToHash(TextFormat("survivor-meleeattack_handgun_%d", i));
		AnimationClipList[2].Frames[i] = hash;
		AnimationClipList[2].SpriteFrames[i] = GetTextureResource(hash);
	}


	AnimationClipList[3] = (AnimationClip)
	{
		.FrameCount = 1,
		.Name = "player_leg_idle",
		.Hash = ToHash("player_leg_idle"),
		.Loop = true
	};

	for (int i = 0; i < 1; i++)
	{
		unsigned long hash = ToHash(TextFormat("survivor-idle_%d", i));
		AnimationClipList[3].Frames[i] = hash;
		AnimationClipList[3].SpriteFrames[i] = GetTextureResource(hash);
	}


	AnimationClipList[4] = (AnimationClip)
	{
		.FrameCount = 20,
		.Name = "player_leg_move",
		.Hash = ToHash("player_leg_move"),
		.Loop = true
	};

	for (int i = 0; i < 20; i++)
	{
		unsigned long hash = ToHash(TextFormat("survivor-walk_%d", i));
		AnimationClipList[4].Frames[i] = hash;
		AnimationClipList[4].SpriteFrames[i] = GetTextureResource(hash);
	}
}

void UnloadAnimationPack()
{
	if (AnimationClipCount > 0)
	{
		MFree(AnimationClipList, AnimationClipCount, sizeof(AnimationClip), "Animation Clip List");
	}
}

AnimationClip* GetAnimationResource(unsigned long hash)
{
	for (int i = 0; i < AnimationClipCount; i++)
	{
		if (AnimationClipList[i].Hash == hash)
			return &AnimationClipList[i];
	}

	return NULL;
}