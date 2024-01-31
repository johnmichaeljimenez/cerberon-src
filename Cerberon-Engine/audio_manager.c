#include <raylib.h>
#include "utils.h"
#include "audio_manager.h"
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <raymath.h>
#include "collision.h"
#include "game.h"
#include "memory.h"
#include <fmod.h>

static FMOD_SYSTEM* audioSystem;
static FMOD_VECTOR listenerPosition;
static Vector2 listenerPositionV;
static FMOD_CHANNEL* channel;
static FMOD_CHANNELGROUP* worldChannelGroup;

const float MAX_AUDIO_RADIUS = 1024;
const float MIN_AUDIO_RADIUS = 200;

void AudioInit()
{
	FMOD_System_Create(&audioSystem, FMOD_VERSION);
	FMOD_System_Init(audioSystem, 512, FMOD_INIT_CHANNEL_LOWPASS, NULL);
	FMOD_System_Set3DSettings(audioSystem, 0.0f, 1, 1.0f);

	FMOD_System_CreateChannelGroup(audioSystem, "World", &worldChannelGroup);

	//TODO: Add a txt file for sound loading reference list
	AudioClipCount = 19;
	AudioClipList = MCalloc(AudioClipCount, sizeof(AudioClip), "Audio Clip List");

	AudioClipList[0] = AudioLoadClip("res/test.wav", true);
	for (int i = 0; i < 9; i++)
	{
		AudioClipList[i + 1] = AudioLoadClip(TextFormat("res/sfx/footstep/%d.ogg", i), true);
	}

	AudioClipList[10] = AudioLoadClip("res/sfx/gunshot.wav", true);

	listenerPosition.x = 0;
	listenerPosition.y = 0;
}

void AudioUnload()
{
	if (AudioClipCount > 0)
	{
		for (int i = 0; i < AudioClipCount; i++)
		{
			FMOD_Sound_Release(AudioClipList[i].Sound);
		}

		MFree(AudioClipList, AudioClipCount, sizeof(AudioClip), "Audio Clip List");
	}

	FMOD_System_Close(audioSystem);
	FMOD_System_Release(audioSystem);
}

AudioClip AudioLoadClip(char* file, bool is3D)
{
	AudioClip a = (AudioClip){
		.Sound = NULL,
		.Hash = 0,
		.Name = NULL,
	};

	FMOD_RESULT result = FMOD_System_CreateSound(audioSystem, file, FMOD_3D_LINEARSQUAREROLLOFF, 0, &a.Sound);
	if (result != FMOD_OK)
		return a;

	strcpy_s(a.Name, 32, GetFileNameWithoutExt(file));
	a.Hash = ToHash(a.Name);
	if (is3D)
	{
		FMOD_Sound_Set3DMinMaxDistance(a.Sound, MIN_AUDIO_RADIUS, MAX_AUDIO_RADIUS);
	}

	return a;
}

void AudioPlay(unsigned long hash, Vector2 pos)
{
	FMOD_VECTOR fmodPos = (FMOD_VECTOR){ pos.x, pos.y };

	for (int i = 0; i < AudioClipCount; i++)
	{
		AudioClip* a = &AudioClipList[i];
		if (a->Hash == hash)
		{
			float dist = Vector2Distance(pos, listenerPositionV);
			if (dist > MAX_AUDIO_RADIUS)
				break;

			float distReverb = Remap(dist, 0, MAX_AUDIO_RADIUS, 1.0f, 0.2f);

			FMOD_RESULT result = FMOD_System_PlaySound(audioSystem, a->Sound, worldChannelGroup, 0, &channel);
			if (result == FMOD_OK)
			{
				FMOD_Channel_SetPitch(channel, GetRandomValueFloat(0.8, 1.2));
				FMOD_Channel_Set3DAttributes(channel, &fmodPos, NULL);
				LinecastHit l;
				bool occluded = Linecast(pos, listenerPositionV, &l, 0);
				FMOD_Channel_SetLowPassGain(channel, occluded ? 0.2f : distReverb);
			}

			break;
		}
	}
}

void AudioUpdateListenerPosition(Vector2 pos)
{
	listenerPositionV = pos;
	listenerPosition.x = pos.x;
	listenerPosition.y = pos.y;
	FMOD_System_Set3DListenerAttributes(audioSystem, 0, &listenerPosition, NULL, NULL, NULL);
}

void AudioUpdate()
{
	FMOD_System_Update(audioSystem);
}