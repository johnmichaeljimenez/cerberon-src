#include "fsm.h"
#include <stdlib.h>
#include "animation_player.h"

FSMState FSMStateCreate(FSM* fsm, char* name, void* data, FSMStateFlags flags, AnimationPlayer* onEnter, AnimationPlayer* onUpdate, AnimationPlayer* onExit)
{
	FSMState state = { 0 };
	state.Data = data;
	state.OnEnter = onEnter;
	state.OnExit = onExit;
	state.OnUpdate = onUpdate;

	return state;
}


void FSMUpdate(FSM* fsm)
{
	if (fsm->CurrentState == NULL)
		return;
}

void FSMSetCurrentState(FSM* fsm, FSMState* state, AnimationPlayer** ref)
{
	state->stateIndex = 0;
	if (state->OnEnter != NULL)
		AnimationPlayerPlay(state->OnEnter, true, ref, true);
}