#pragma once

typedef struct FSMState
{
	FSM* FSM;
	bool IsValid;
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