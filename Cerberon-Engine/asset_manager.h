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
	unsigned long Hash;
	bool Loop;

	int FrameCount;

	//64 frames fixed max frame count on animation
	unsigned long Frames[64];
	TextureResource* SpriteFrames[64];
} AnimationClip;

NPatchInfo WallNPatch;
TextureResource* WallTexture;

TextureResource* LightTexture;
TextureResource* FlashlightTexture;

int TextureResourceCount;
TextureResource* TextureResourceList;

int AnimationClipCount;
AnimationClip* AnimationClipList;

void LoadResources();
void UnloadResources();
void LoadTexturePack(char* filename, int* arrayCount, TextureResource** texArray, TextureType type);
void UnloadTexturePack(int* arrayCount, TextureResource** array);
TextureResource* GetTextureResource(unsigned long hash);

void LoadAnimationPack(char* filename);
void UnloadAnimationPack();
AnimationClip* GetAnimationResource(unsigned long hash);