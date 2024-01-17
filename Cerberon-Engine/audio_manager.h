#pragma once
#include <raylib.h>
#include <fmod.h>

typedef struct AudioClip
{
	FMOD_SOUND* Sound;
	char* Name[32];
	unsigned long Hash;
} AudioClip;

int AudioClipCount;
AudioClip* AudioClipList;

void AudioInit();
void AudioUnload();
AudioClip AudioLoadClip(char* file, bool is3D);
void AudioUpdate();
void AudioPlay(unsigned long hash, Vector2 pos);
void AudioUpdateListenerPosition(Vector2 pos);