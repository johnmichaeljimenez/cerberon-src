#pragma once
#include "asset_manager.h"

typedef enum AnimationFlags
{
	AnimationFlag_None = 1 << 0,
	AnimationFlag_Physics = 1 << 1,
	AnimationFlag_CanAttack = 1 << 2,
	AnimationFlag_CannotBeInterrupted = 1 << 3,
	AnimationFlag_DisableMovement = 1 << 4
} AnimationFlags;

typedef struct AnimationPlayerGroup
{
	struct AnimationPlayer* Animations[32];
	struct AnimationPlayer* CurrentAnimation;
} AnimationPlayerGroup;

typedef struct AnimationPlayer
{
	AnimationPlayerGroup* Group;
	AnimationClip* Clip;
	struct AnimationPlayer* NextAnimation;
	float FrameRate;
	int CurrentFrame;
	bool Paused;
	AnimationFlags Flags;
	
	float _timer;
	float NormalizedTime;
	bool isPlaying;

	void(*OnStart)();
	void(*OnFrameChanged)();
	void(*OnEnd)();
} AnimationPlayer;

AnimationPlayer AnimationPlayerCreate(AnimationPlayerGroup* group, AnimationClip* clip, AnimationFlags flags, void(*onStart)(), void(*OnFrameChanged)(), void(*onEnd)(), int frameRate);
void AnimationPlayerUpdate(AnimationPlayer* a);
void AnimationPlayerPlay(AnimationPlayerGroup* a, AnimationPlayer* p);