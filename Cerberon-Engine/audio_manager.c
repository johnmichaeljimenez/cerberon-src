#include <raylib.h>
#include "audio_manager.h"
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <raymath.h>

static Sound testSound;
static Sound soundsAlias[8];

void AudioInit()
{
	testSound = LoadSound("res/test.wav");
	for (int i = 0; i < 8; i++)
	{
		soundsAlias[i] = LoadSoundAlias(testSound);
	}
}

void AudioUpdate()
{
	for (int i = 0; i < 16; i++)
	{
		AudioSource* a = &AudioSourceWorldList[i];
		if (a->SoundData == NULL)
			continue;

		if (!a->IsPlaying)
			continue;

		if (a->IsPlaying && (a->SoundData == NULL || !IsSoundPlaying(*a->SoundData)))
		{
			a->IsPlaying = false;
			a->SoundData = NULL;
			continue;
		}

		if (!a->Is3D)
			continue;

		Vector2 diff = Vector2Subtract(AudioListenerPosition, a->Position);
		a->OutVolume = Remap(Vector2Length(diff), 0, a->Radius, 1, 0);

		diff = Vector2Normalize(diff);
		a->OutPan = Remap(diff.x, -1, 1, 0, 1);

		SetSoundVolume(*a->SoundData, a->OutVolume * a->Volume);
		SetSoundPan(*a->SoundData, a->OutPan);
	}
}

void AudioUnload()
{
	for (int i = 0; i < 8; i++)
	{
		UnloadSoundAlias(soundsAlias[i]);
	}

	UnloadSound(testSound);
}

static Sound* GetSoundFromAlias(Sound* list, int count)
{
	for (int i = 0; i < count; i++)
	{
		if (!IsSoundPlaying(list[i]))
		{
			return &list[i];
		}
	}

	return NULL;
}

bool AudioPlay(int hash, Vector2 position)
{
	for (int i = 0; i < 16; i++)
	{
		AudioSource* a = &AudioSourceWorldList[i];
		if (a->IsPlaying)
			continue;

		a->SoundData = GetSoundFromAlias(soundsAlias, 8); //use real audio resource
		a->Position = position;
		a->Radius = 512;
		a->Is3D = true;
		a->Volume = 1;
		a->IsPlaying = true;

		PlaySound(*a->SoundData);
		
		//a->Clip

		return true;
	}

	return false; //ran out of empty audio channels
}