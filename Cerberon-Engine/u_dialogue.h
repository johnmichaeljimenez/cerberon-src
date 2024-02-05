#pragma once
#include "ui_manager.h"

UIElement* UDialoguePanel;
char UDialogueText[256];

void UDialogueCreate();
void UDialogueUpdate(UIElement* u);
void UDialogueShow(UIElement* u);
void UDialogueHide(UIElement* u);
void UDialogueDraw(UIElement* u);