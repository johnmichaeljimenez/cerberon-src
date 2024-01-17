#include <raylib.h>
#include "utils.h"
#include "audio_manager.h"
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <raymath.h>
#include "collision.h"
#include "game.h"
#include <fmod.h>

static FMOD_SYSTEM* audioSystem;
static FMOD_VECTOR listenerPosition;

void AudioInit()
{
	FMOD_System_Create(&audioSystem, FMOD_VERSION);
	FMOD_System_Init(audioSystem, 512, FMOD_INIT_NORMAL, NULL);
	FMOD_System_Set3DSettings(audioSystem, 0.0f, 1, 1.0f);
}

void AudioUnload()
{
	FMOD_System_Close(audioSystem);
	FMOD_System_Release(audioSystem);
}

AudioClip* AudioLoadClip(char* file, bool is3D)
{
	AudioClip a = { 0 };
	FMOD_RESULT result = FMOD_System_CreateSound(system, file, is3D? FMOD_3D_LINEARSQUAREROLLOFF : FMOD_DEFAULT, 0, a.Sound);
	if (result != FMOD_OK)
		return NULL;

	strcpy_s(a.Name, 32, GetFileNameWithoutExt(file));
	if (is3D)
	{
		FMOD_Sound_Set3DMinMaxDistance(a.Sound, 64, 512);
	}

	return &a;
}

void AudioPlay(char* id, Vector2 pos)
{

}

void AudioUpdateListenerPosition(Vector2 pos)
{
	listenerPosition.x = pos.x;
	listenerPosition.y = pos.y;
}

void AudioUpdate()
{

}