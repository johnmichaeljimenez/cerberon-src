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
	unsigned long Hash;
	Texture2D Texture;
	TextureType TextureType;
} TextureResource;

typedef struct AnimationClip
{
	char* Name[32];
	int FrameCount;
	unsigned long Hash;
	unsigned long* Frames;
	bool Loop;
} AnimationClip;

NPatchInfo WallNPatch;
TextureResource* WallTexture;

TextureResource* LightTexture;

int TextureResourceCount;
TextureResource* TextureResourceList;

int AnimationClipCount;
AnimationClip* AnimationClipList;

void LoadResources();
void UnloadResources();
void LoadTexturePack(char* filename, int* arrayCount, TextureResource** array);
void UnloadTexturePack(int* arrayCount, TextureResource** array);
TextureResource* GetTextureResource(unsigned long hash);

void LoadAnimationPack(char* filename, int* arrayCount, AnimationClip** clipArray);
void UnloadAnimationPack(int* arrayCount, AnimationClip** clipA);
AnimationClip* GetAnimationResource(unsigned long hash);