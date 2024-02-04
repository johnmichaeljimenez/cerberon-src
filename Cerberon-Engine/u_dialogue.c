#include <raylib.h>
#include "ui_manager.h"
#include "u_dialogue.h"
#include "utils.h"
#include <rlgl.h>

static UIElement* uiDialogueBG;
static UIElement* uiDialogueText;
static UIElement* uiDialogueButton;

static float t;

static void OnDialogueClick(UIElement* e)
{
	UIHide(uiDialogueBG);
}

void UDialogueCreate()
{
	uiDialogueBG = UIFindElement("DialogueBG");
	uiDialogueText = UIFindElement("DialogueText");
	uiDialogueButton = UIFindElement("DialogueButton");

	uiDialogueBG->OnDraw = UDialogueDraw;
	uiDialogueBG->OnShow = UDialogueShow;
	uiDialogueBG->OnHide = UDialogueHide;

	uiDialogueButton->OnClick = OnDialogueClick;
}

void UDialogueUpdate(UIElement* u)
{

}

void UDialogueShow(UIElement* u)
{
	t = 0;
}

void UDialogueHide(UIElement* u)
{
	t = 0;
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