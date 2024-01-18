#pragma once
#include "asset_manager.h"

typedef struct AnimationPlayer
{
	int ClipCount;
	AnimationClip* Clips;
	float FrameRate;
	int CurrentFrame;
	
	float _timer;

	void(*OnStart)();
	void(*OnFrameChanged)();
	void(*OnEnd)();
} AnimationPlayer;

void AnimationPlayerUpdate(AnimationPlayer* a);