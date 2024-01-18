#pragma once
#include "asset_manager.h"

typedef struct AnimationPlayer
{
	AnimationClip* Clip;
	float FrameRate;
	int CurrentFrame;
	bool Paused;
	
	float _timer;

	void(*OnStart)();
	void(*OnFrameChanged)();
	void(*OnEnd)();
} AnimationPlayer;

AnimationPlayer CreateAnimationPlayer(AnimationClip *clip);
void AnimationPlayerUpdate(AnimationPlayer* a);