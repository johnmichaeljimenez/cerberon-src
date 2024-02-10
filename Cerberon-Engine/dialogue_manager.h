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

Dialogue* DialogueCurrentItem;
Dialogue* DialogueCurrentList[32];
int DialogueCurrentCount;
int DialogueCurrentIndex;
void(*DialogueCurrentOnDone)();

void DialogueInit();
void DialogueShow(char* id, void(*onDone)());