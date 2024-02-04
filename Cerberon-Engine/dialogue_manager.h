#pragma once

typedef struct Dialogue
{
	char ID[16];
	char Message[256];
} Dialogue;

Dialogue DialogueList[256];

void DialogueInit();
void DialogueShow(char* id);