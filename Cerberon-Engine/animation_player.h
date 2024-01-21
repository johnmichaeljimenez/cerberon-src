#pragma once
#include "asset_manager.h"

typedef struct AnimationPlayerGroup
{
	struct AnimationPlayer* Animations[32];
	struct AnimationPlayer* CurrentAnimation;
} AnimationPlayerGroup;

typedef struct AnimationPlayer
{
	AnimationPlayerGroup* Group;
	AnimationClip* Clip;
	float FrameRate;
	int CurrentFrame;
	bool Paused;
	
	float _timer;
	float NormalizedTime;
	bool isPlaying;

	void(*OnStart)();
	void(*OnFrameChanged)();
	void(*OnEnd)();
} AnimationPlayer;

AnimationPlayer AnimationPlayerCreate(AnimationClip* clip, void(*onStart)(), void(*OnFrameChanged)(), void(*onEnd)(), int frameRate);
void AnimationPlayerUpdate(AnimationPlayer* a);
void AnimationPlayerPlay(AnimationPlayerGroup* a, AnimationPlayer* p);