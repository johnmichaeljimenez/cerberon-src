#include "animation_player.h"
#include "time.h"

void AnimationPlayerUpdate(AnimationPlayer* a)
{
	if (a->Paused)
		return;

	if (!a->Clip->Loop && a->CurrentFrame >= a->Clip->FrameCount)
		return;

	bool changed = false;
	bool ended = false;
	float rate = 1.0f / a->FrameRate;

	a->_timer += TICKRATE;
	while (a->_timer > rate) //make sure to process all upcoming frames regardless of lag
	{
		a->_timer -= rate;
		a->CurrentFrame++;

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
		a->OnEnd();
}