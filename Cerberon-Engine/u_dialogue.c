#include <raylib.h>
#include "ui_manager.h"
#include "u_dialogue.h"
#include "utils.h"
#include "dialogue_manager.h"
#include <rlgl.h>
#include <string.h>

static UIElement* uiDialogueText;
static UIElement* uiDialogueButton;

static float t;
static int n;
static bool animate;
static char display[256];

static void Refresh()
{
	strcpy_s(display, 256, DialogueCurrentItem);
	t = 0.1;
	n = 0;
	animate = true;

	memset(display, 0, 255);
}

static void OnDialogueClick(UIElement* e)
{
	DialogueCurrentIndex++;
	if (DialogueCurrentIndex >= DialogueCurrentCount)
	{
		UIHide(UDialoguePanel);
		return;
	}

	DialogueCurrentItem = DialogueCurrentList[DialogueCurrentIndex];
	Refresh();
}

void UDialogueCreate()
{
	UDialoguePanel = UIFindElement("DialogueBG");
	uiDialogueText = UIFindElement("DialogueText");
	uiDialogueButton = UIFindElement("DialogueButton");

	UDialoguePanel->OnDraw = UDialogueDraw;
	UDialoguePanel->OnShow = UDialogueShow;
	UDialoguePanel->OnHide = UDialogueHide;
	UDialoguePanel->OnUpdate = UDialogueUpdate;

	uiDialogueButton->OnClick = OnDialogueClick;
}

void UDialogueUpdate(UIElement* u)
{
	if (animate && t > 0 && DecrementTimer(&t, 0, 1, false))
	{
		t = 0.008;
		if (DialogueCurrentItem->Message[n] == 0)
		{
			display[n] = 0;
			animate = false;
			return;
		}

		display[n] = DialogueCurrentItem->Message[n];
		if (display[n] == '.' || display[n] == '?')
			t = 0.1;

		n++;
	}
}

void UDialogueShow(UIElement* u)
{
	Refresh();
}

void UDialogueHide(UIElement* u)
{
	t = 0;
	n = 0;
}

void UDialogueDraw(UIElement* u)
{
	DrawRectangleRec(u->Rect.Rectangle, (Color) { 255, 255, 255, 50 });

	/*u = uiDialogueText;
	DrawText(TextFormat("%.2f, %.2f %.2f, %.2f = %.2f, %.2f, %.2f, %.2f %d",
		u->Rect.Min.x, u->Rect.Min.y, u->Rect.Max.x, u->Rect.Max.y,
		u->Rect.Rectangle.x, u->Rect.Rectangle.y, u->Rect.Rectangle.width, u->Rect.Rectangle.height,
		u->Hovered? 1 : 0
	), 12, 300, 15, WHITE);*/

	DrawTextRect(display, uiDialogueText->Rect.Rectangle, 12, true, WHITE);
	DrawRectangleRec(uiDialogueButton->Rect.Rectangle, uiDialogueButton->Hovered ? RED : WHITE);
}