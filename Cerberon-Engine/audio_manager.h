#pragma once
#include <raylib.h>
#include "asset_manager.h"

typedef struct AudioSource
{
	SoundResource* Clip;
	Vector2 Position;
	float Radius;
	bool Is3D;
} AudioSource;

AudioSource* AudioSourceWorldList[16];

void AudioInit();
void AudioUpdate();
void AudioUnload();
void AudioPlay();