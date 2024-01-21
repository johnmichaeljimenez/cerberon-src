#pragma once
#include <stdbool.h>

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

	void* Data;

	void(*OnInit)(struct FSMState* state);
	void(*OnEnter)(struct FSMState* state);
	void(*OnUpdate)(struct FSMState* state);
	void(*OnExit)(struct FSMState* state);

} FSMState;

typedef struct FSM
{
	FSMState States[16];
	FSMState* CurrentState;
} FSM;

FSMState FSMStateCreate(FSM* fsm, char* name, void* data, FSMStateFlags flags,
	void(*onInit)(FSMState* s),
	void(*onEnter)(FSMState* s),
	void(*onUpdate)(FSMState* s),
	void(*onExit)(FSMState* s)
);

void FSMUpdate(FSM* fsm);