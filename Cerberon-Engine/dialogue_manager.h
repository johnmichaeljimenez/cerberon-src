#pragma once

typedef struct Dialogue
{
	bool IsValid;
	char ID[16];
	char Message[256];
} Dialogue;

Dialogue* DialogueList;
int DialogueSize;
int DialogueCount;

void DialogueInit();
void DialogueShow(char* id);