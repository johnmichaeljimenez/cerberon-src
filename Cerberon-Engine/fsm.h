#pragma once
#include <stdbool.h>
#include "animation_player.h"

typedef enum FSMStateFlags
{
	FSMFlag_None = 1 << 0,
	FSMFlag_Physics = 1 << 1,
	FSMFlag_CanAttack = 1 << 2,
	FSMFlag_CannotBeInterrupted = 1 << 3,
	FSMFlag_DisableMovement = 1 << 4
} FSMStateFlags;

typedef struct FSMState
{
	struct FSM* FSM;
	bool IsValid;
	FSMStateFlags Flags;
	char* Name[32];
	unsigned long Hash;

	int stateIndex; //0 = start, 1 = middle/loop, 2 = end

	void* Data;

	AnimationPlayer* OnEnter;
	AnimationPlayer* OnUpdate;
	AnimationPlayer* OnExit;

} FSMState;

typedef struct FSM
{
	FSMState States[16];
	FSMState* CurrentState;
} FSM;

FSMState FSMStateCreate(FSM* fsm, char* name, void* data, FSMStateFlags flags, AnimationPlayer* onEnter, AnimationPlayer* onUpdate, AnimationPlayer* onExit);

void FSMUpdate(FSM* fsm);
void FSMSetCurrentState(FSM* fsm, FSMState* state, AnimationPlayer** ref);