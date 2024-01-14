#pragma once
#include <raylib.h>
#include "asset_manager.h"

typedef struct AudioSource
{
	bool IsPlaying;
	AudioStream* Stream;
	SoundResource* Clip;
	Vector2 Position;
	float Volume;
	float Radius;
	bool Is3D;

	float OutVolume;
	float OutPan;
} AudioSource;

Vector2 AudioListenerPosition;
AudioSource* AudioSourceWorldList[16];

void AudioInit();
void AudioUpdate();
void AudioUnload();
void AudioPlay();