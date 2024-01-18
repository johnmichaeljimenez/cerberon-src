#include "animation_player.h"
#include "time.h"

AnimationPlayer AnimationPlayerCreate(AnimationClip* clip, void(*onStart)(), void(*OnFrameChanged)(), void(*onEnd)(), int frameRate)
{
	AnimationPlayer a = { 0 };

	a.Clip = clip;
	a.CurrentFrame = 0;
	a.FrameRate = frameRate;
	a.Paused = false;
	a._timer = 0;
	a.OnStart = onStart;
	a.OnFrameChanged = OnFrameChanged;
	a.OnEnd = onEnd;

	return a;
}

void AnimationPlayerUpdate(AnimationPlayer* a)
{
	if (a->Paused)
		return;

	if (!a->Clip->Loop && a->CurrentFrame >= a->Clip->FrameCount)
		return;

	bool changed = false;
	bool ended = false;
	float rate = 1.0f / a->FrameRate;

	a->_timer += GetFrameTime();
	while (a->_timer > rate) //make sure to process all upcoming frames regardless of lag
	{
		a->_timer -= rate;
		a->CurrentFrame++;

		if (a->OnFrameChanged != NULL)
			a->OnFrameChanged();

		if (a->CurrentFrame >= a->Clip->FrameCount)
		{
			if (a->Clip->Loop)
				a->CurrentFrame = 0;
			else
				ended = true;
		}
	}

	if (ended && a->OnEnd != NULL)
		a->OnEnd();
}

void AnimationPlayerPlay(AnimationPlayer* a, bool resetFromStart, AnimationPlayer** ref, bool allowReplay)
{
	if (!allowReplay && *ref == a)
		return;

	*ref = a;
	if (resetFromStart)
	{
		a->CurrentFrame = 0;
		a->_timer = 0;
		if (a->OnStart != NULL)
			a->OnStart();
	}

	a->Paused = false;
}