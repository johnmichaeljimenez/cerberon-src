#include <raylib.h>
#include "ui_manager.h"
#include "u_dialogue.h"
#include "utils.h"

static UIElement* uiDialogueBG;
static UIElement* uiDialogueText;
static UIElement* uiDialogueButton;

void UDialogueCreate()
{
	uiDialogueBG = UICreateElement(NULL, false,
		(Vector2){ 64, 128 },
		(Vector2){ 64, 12 },
		(Vector2){ 1, 0 },
		(Vector2){ 1, 1 },
		false, true
	);

	uiDialogueText = UICreateElement(uiDialogueBG, false,
		(Vector2){ 12, 12 },
		(Vector2){ 12, 12 },
		(Vector2){ 1, 1 },
		(Vector2){ 1, 1 },
		false, false
	);

	uiDialogueButton = UICreateElement(uiDialogueBG, true,
		(Vector2){ -32, -16 },
		(Vector2){ 64, 32 },
		(Vector2){ 1, 1 },
		(Vector2){ 0, 0 },
		true, false
	);

	uiDialogueBG->OnDraw = UDialogueDraw;
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
	DrawRectangleRec(u->Rect.Rectangle, (Color){255,255,255, 50});

	/*u = uiDialogueText;
	DrawText(TextFormat("%.2f, %.2f %.2f, %.2f = %.2f, %.2f, %.2f, %.2f %d",
		u->Rect.Min.x, u->Rect.Min.y, u->Rect.Max.x, u->Rect.Max.y,
		u->Rect.Rectangle.x, u->Rect.Rectangle.y, u->Rect.Rectangle.width, u->Rect.Rectangle.height,
		u->Hovered? 1 : 0
	), 12, 300, 15, WHITE);*/

	DrawTextRect("Hello!", uiDialogueText->Rect.Rectangle, 12, true, WHITE);
	DrawRectangleRec(uiDialogueButton->Rect.Rectangle, uiDialogueButton->Hovered ? RED : WHITE);
}