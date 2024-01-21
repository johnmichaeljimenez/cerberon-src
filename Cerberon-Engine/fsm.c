#include "fsm.h"
#include <stdlib.h>

FSMState FSMStateCreate(FSM* fsm, char* name, void* data, FSMStateFlags flags,
	void(*onInit)(FSMState* s),
	void(*onEnter)(FSMState* s),
	void(*onUpdate)(FSMState* s),
	void(*onExit)(FSMState* s)
)
{

}


void FSMUpdate(FSM* fsm)
{
	if (fsm->CurrentState != NULL)
		fsm->CurrentState->OnUpdate(fsm->CurrentState);
}