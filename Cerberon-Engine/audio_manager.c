#include <raylib.h>
#include "audio_manager.h"
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <raymath.h>
#include "collision.h"
#include "game.h"
#include <fmod.h>

static FMOD_SYSTEM* audioSystem;

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

void AudioPlay(char* id, Vector2 pos)
{

}

void AudioUpdateListenerPosition(Vector2 pos)
{

}

void AudioUpdate()
{

}