#pragma warning(disable:4996)
#include <raylib.h>
#include <raymath.h>
#include "dialogue_manager.h"
#include "memory.h"
#include <string.h>
#include "ui_manager.h"
#include "u_dialogue.h"
#include <stdio.h>

void DialogueInit()
{
	DialogueCount = 0;
	FILE* file = fopen("res/data/dialogue.tsv", "rb");

	char buffer[100];

	DialogueSize = 512;
	DialogueList = MCalloc(DialogueSize, sizeof(Dialogue), "Dialogue List");

	int i = 0;
	while (fgets(buffer, sizeof(buffer), file) != NULL) {
		if (buffer[0] == '\n' || buffer[0] == '\0') {
			continue;
		}

		Dialogue d;
		int res = sscanf(buffer, "%16[^\t]\t%256[^\t]\n",
			&d.ID, &d.Message
		);

		if (res == 0) {
			fprintf(stderr, "Error reading line %d from the file\n", i + 1);
			continue;
		}

		DialogueList[i] = d;
		DialogueCount++;
		i++;
	}

	fclose(file);
}

void DialogueShow(char* id, void(*onDone)())
{
	bool hasDialogue = false;

	DialogueCurrentIndex = 0;
	DialogueCurrentCount = 0;
	DialogueCurrentItem = NULL;
	DialogueCurrentOnDone = NULL;

	for (int i = 0; i < DialogueCount; i++)
	{
		Dialogue* d = &DialogueList[i];
		if (d == NULL || !d->IsValid)
			continue;

		if (strcmp(id, d->ID) == 0)
		{
			DialogueCurrentList[DialogueCurrentCount] = d;
			DialogueCurrentCount++;
			hasDialogue = true;
		}
	}

	if (hasDialogue)
	{
		DialogueCurrentOnDone = onDone;
		DialogueCurrentItem = DialogueCurrentList[0];
		UIShow(UDialoguePanel);
	}
}