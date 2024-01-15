#pragma once
#include <raylib.h>
#include "asset_manager.h"

typedef struct AudioSource
{
	bool IsPlaying;
	Sound* SoundData; //SoundResource alias
	SoundResource* Clip;
	Vector2 Position;
	float Volume;
	float Radius;
	bool Is3D;

	float OutVolume;
	float OutPan;

	float _t;
	bool _occluded;
} AudioSource;

Vector2 AudioListenerPosition;
AudioSource AudioSourceWorldList[16];

void AudioInit();
void AudioUpdate();
void AudioUnload();
bool AudioPlay(int hash, Vector2 position);