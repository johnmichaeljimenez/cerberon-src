#include <raylib.h>
#include "audio_manager.h"
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <raymath.h>

void AudioInit()
{

}

void AudioUpdate()
{
	for (int i = 0; i < 16; i++)
	{
		AudioSource* a = &AudioSourceWorldList[i];
		if (a->Stream == NULL)
			continue;

		if (!a->IsPlaying)
			continue;

		if (!a->Is3D)
			continue;

		Vector2 diff = Vector2Subtract(a->Position, AudioListenerPosition);
		a->OutVolume = Remap(Vector2Length(diff), 0, a->Radius, 1, 0);

		diff = Vector2Normalize(diff);
		a->OutPan = Remap(diff.x, -1, 1, 0, 1);

		SetAudioStreamPan(*a->Stream, a->OutVolume);
		SetAudioStreamPan(*a->Stream, a->OutPan);
	}
}

void AudioUnload()
{

}

void AudioPlay()
{

}