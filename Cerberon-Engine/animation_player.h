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

AnimationPlayer AnimationPlayerCreate(AnimationClip* clip, void(*onStart)(), void(*OnFrameChanged)(), void(*onEnd)(), int frameRate);
void AnimationPlayerUpdate(AnimationPlayer* a);
void AnimationPlayerPlay(AnimationPlayer* a, bool resetFromStart, AnimationPlayer** ref, bool allowReplay);