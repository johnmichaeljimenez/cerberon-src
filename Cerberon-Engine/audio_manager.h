#pragma once
#include <raylib.h>
#include "asset_manager.h"
#include <fmod.h>

typedef struct AudioClip
{
	FMOD_SOUND* Sound;
	char* name[32];
} AudioClip;

void AudioInit();
void AudioUnload();
void AudioUpdate();
void AudioPlay(char* id, Vector2 pos);
void AudioUpdateListenerPosition(Vector2 pos);