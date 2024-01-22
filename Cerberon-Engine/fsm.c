#include "fsm.h"
#include <stdlib.h>
#include "animation_player.h"

FSMState FSMStateCreate(FSM* fsm, char* name, FSMStateFlags flags, AnimationPlayer* onEnter, AnimationPlayer* onUpdate, AnimationPlayer* onExit)
{
	FSMState state = { 0 };
	state.OnEnter = onEnter;
	state.OnExit = onExit;
	state.OnUpdate = onUpdate;

	return state;
}


void FSMUpdate(FSM* fsm)
{
	if (fsm->CurrentState == NULL)
		return;

	FSMState* state = fsm->CurrentState;
	if (state == 0)
	{
		if (state->OnEnter != NULL && state->OnEnter->isPlaying)
			return;

		state++;
	}
	else if (state == 1)
	{
		if (state->OnEnter != NULL && state->OnEnter->isPlaying)
			return;

		state++;
	}
	else
	{
		if (state->OnEnter != NULL && state->OnEnter->isPlaying)
			return;


	}
}

void FSMSetCurrentState(FSM* fsm, FSMState* state)
{
	state->stateIndex = 0;
	if (state->OnEnter != NULL)
		AnimationPlayerPlay(fsm->AnimationPlayerGroup, state->OnEnter);
}