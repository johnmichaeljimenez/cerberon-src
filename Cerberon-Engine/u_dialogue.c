#include <raylib.h>
#include "ui_manager.h"
#include "u_dialogue.h"

void UDialogueCreate()
{
	UIElement* e = UICreateElement(NULL, false,
		(Vector2){ 4, 4 },
		(Vector2){ 4, 4 },
		(Vector2){ 1, 0 },
		(Vector2){ 1, 1 }
	);

	e->OnDraw = UDialogueDraw;
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
	DrawRectangleRec(u->Rect.Rectangle, WHITE);
}