#include <raylib.h>
#include "ui_manager.h"
#include "u_dialogue.h"

void UDialogueCreate()
{
	UIElement* e = UICreateElement(NULL, false,
		(Vector2){ 64, 128 },
		(Vector2){ 64, 12 },
		(Vector2){ 1, 0 },
		(Vector2){ 1, 1 },
		false
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
	//DrawText(TextFormat("%.2f, %.2f %.2f, %.2f = %.2f, %.2f, %.2f, %.2f",
	//	u->Rect.Min.x, u->Rect.Min.y, u->Rect.Max.x, u->Rect.Max.y,
	//	u->Rect.Rectangle.x, u->Rect.Rectangle.y, u->Rect.Rectangle.width, u->Rect.Rectangle.height
	//), 12, 300, 15, WHITE);
	DrawRectangleRec(u->Rect.Rectangle, WHITE);
}