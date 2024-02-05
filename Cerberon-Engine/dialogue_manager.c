#include <raylib.h>
#include <raymath.h>
#include "dialogue_manager.h"
#include "memory.h"
#include <string.h>
#include "ui_manager.h"
#include "u_dialogue.h"

void DialogueInit()
{
	DialogueCount = 0;
	DialogueSize = 512;
	DialogueList = MCalloc(DialogueSize, sizeof(Dialogue), "Dialogue List");
}

void DialogueShow(char* id)
{
	for (int i = 0; i < DialogueCount; i++)
	{
		Dialogue* d = &DialogueList[i];
		if (d == NULL || !d->IsValid)
			continue;

		if (strcmp(id, d->ID) == 0)
		{
			strcpy_s(UDialogueText, 256, d->Message);
			UIShow(UDialoguePanel);
			return;
		}
	}
}