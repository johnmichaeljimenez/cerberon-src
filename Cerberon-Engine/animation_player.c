#include "animation_player.h"
#include "time.h"

AnimationPlayer AnimationPlayerCreate(AnimationPlayerGroup* group, AnimationClip* clip, void(*onStart)(), void(*OnFrameChanged)(), void(*onEnd)(), int frameRate)
{
	AnimationPlayer a = { 0 };

	a.Group = group;
	a.Clip = clip;
	a.CurrentFrame = 0;
	a.FrameRate = frameRate;
	a.Paused = false;
	a._timer = 0;
	a.OnStart = onStart;
	a.OnFrameChanged = OnFrameChanged;
	a.OnEnd = onEnd;
	a.NextAnimation = NULL;

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
	while (a->_timer >= rate) //make sure to process all upcoming frames regardless of lag
	{
		a->_timer -= rate;
		a->CurrentFrame++;
		a->NormalizedTime = (float)a->CurrentFrame / (float)a->Clip->FrameCount;

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

	if (ended)
	{
		if (a->OnEnd != NULL)
			a->OnEnd();

		if (a->NextAnimation != NULL)
			AnimationPlayerPlay(a->Group, a->NextAnimation);
	}
}

void AnimationPlayerPlay(AnimationPlayerGroup* a, AnimationPlayer* p)
{
	if (a == p)
		return;

	a->CurrentAnimation = p;
	p->CurrentFrame = 0;
	p->_timer = 0;
	p->Paused = false;
	p->NormalizedTime = 0;
}