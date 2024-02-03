#include <raylib.h>
#include "ui_manager.h"
#include "u_dialogue.h"

void UDialogueInit(UIElement* u)
{

}

void UDialogueUpdate(UIElement* u)
{

}

void UDialogueShow(UIElement* u)
{

}

void UDialogueHide(UIElement* u)
{

}

void UDialogueDraw(UIElement* u)
{
	DrawRectangle(12, GetScreenHeight() - 212, GetScreenWidth() - 24, 200, (Color){255,255,255,50});
	DrawText("hello", 24, GetScreenHeight() - 200, 25, WHITE);
}