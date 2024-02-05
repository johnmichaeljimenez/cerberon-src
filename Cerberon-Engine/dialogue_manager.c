#include <raylib.h>
#include <raymath.h>
#include "dialogue_manager.h"
#pragma warning(disable:4996)
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

	DialogueSize = 0;
	while (fgets(buffer, sizeof(buffer), file) != NULL) {
		DialogueSize++;
	}

	DialogueSize = 512;
	DialogueList = MCalloc(DialogueSize, sizeof(Dialogue), "Dialogue List");

	fseek(file, 0, SEEK_SET);

	for (int i = 0; i < DialogueSize; i++) {
		Dialogue d;
		int res = fscanf(file, "%16[^\t]\t%256[^\t]\n",
			&d.ID, &d.Message
		);

		if (res == 0) {
			fprintf(stderr, "Error reading line %d from the file\n", i + 1);
			continue;
		}

		DialogueList[i] = d;
		DialogueCount++;
	}

	fclose(file);
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