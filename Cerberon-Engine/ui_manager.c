#pragma warning(disable:4996)
#include <raylib.h>
#include <raymath.h>
#include "ui_manager.h"
#include "u_dialogue.h"
#include <string.h>
#include "utils.h"
#include <stdio.h>

void UILoadData()
{
	FILE* file = fopen("res/data/gui_layout.tsv", "rb");

	int lineCount = 0;
	char buffer[100];

	while (fgets(buffer, sizeof(buffer), file) != NULL) {
		lineCount++;
	}

	fseek(file, 0, SEEK_SET);

	char id[32];
	char idparent[32];
	int clickable = 0;
	Vector2 min = Vector2One(), max = Vector2One(), aMin = Vector2One(), aMax = Vector2One();
	int anchorOrigin = 0;
	int isPanel = 0;

	for (int i = 0; i < lineCount; i++) {
		int res = fscanf(file, "%31[^\t]\t%31[^\t]\t%d\t%f\t%f\t%f\t%f\t%f\t%f\t%f\t%f\t%d\t%d\n",
			id, idparent, &clickable, &min.x, &min.y, &max.x, &max.y, &aMin.x, &aMin.y, &aMax.x, &aMax.y, &anchorOrigin, &isPanel
		);

		if (res == 0) {
			fprintf(stderr, "Error reading line %d from the file\n", i + 1);
			continue;
		}

		UICreateElement(id, UIFindElement(idparent), clickable, min, max, aMin, aMax, anchorOrigin, isPanel);
	}

	fclose(file);
}

void UIInit()
{
	UDialogueCreate();
}

void UISetSelection(UIElement* c)
{
	if (UICurrentSelected != NULL)
	{
		UICurrentSelected->Selected = false;
		if (UICurrentSelected->OnDeselect != NULL)
			UICurrentSelected->OnDeselect(UICurrentSelected);
	}

	UICurrentSelected = c;

	if (c != NULL)
	{
		c->Selected = true;

		if (c->OnSelect != NULL)
		{
			c->OnSelect(c);
		}
	}
}

void UIUpdate()
{
	UIElement* newHovered = NULL;
	Vector2 mousePos = GetMousePosition();

	for (int i = 0; i < UINextElementSlot; i++)
	{
		UIElement* c = &UIElementList[i];
		if (c == NULL)
			continue;

		c->Hovered = false;
		if (!c->IsValid || !c->IsVisible || !c->Clickable || (!c->IsOpen && c->IsMainPanel))
			continue;

		if (CheckCollisionPointRec(mousePos, c->Rect.Rectangle))
		{
			newHovered = c;
		}
	}

	if (newHovered != NULL)
	{
		if (UICurrentHovered != NULL)
			UICurrentHovered->Hovered = false;

		UICurrentHovered = newHovered;
		UICurrentHovered->Hovered = true;
	}

	if (IsKeyPressed(KEY_Q))
	{
		UIElement* p = &UIElementList[0];
		if (p->IsOpen)
			UIHide(p);
		else
			UIShow(p);
	}
}

void UIDraw()
{
	for (int i = 0; i < UINextElementSlot; i++)
	{
		UIElement* c = &UIElementList[i];
		if (c == NULL)
			continue;

		if (!c->IsValid || !c->IsVisible || (!c->IsOpen && c->IsMainPanel))
			continue;

		if (c->OnDraw != NULL)
			c->OnDraw(c);
	}
}

static bool IsAnyUIVisible()
{
	for (int i = 0; i < UINextElementSlot; i++)
	{
		UIElement* c = &UIElementList[i];
		if (c == NULL)
			continue;

		if (!c->IsValid)
			continue;

		if (c->IsOpen)
			return true;
	}

	return false;
}

void UIShow(UIElement* c)
{
	if (c == NULL || !c->IsValid)
		return;

	c->IsOpen = true;
	UIIsVisible = IsAnyUIVisible();
	if (!UIIsVisible)
		return;

	if (c->OnShow != NULL)
		c->OnShow(c);
}

void UIHide(UIElement* c)
{
	if (c == NULL || !c->IsValid)
		return;

	c->IsOpen = false;
	UIIsVisible = IsAnyUIVisible();
	if (!UIIsVisible)
		return;

	if (c->OnHide != NULL)
		c->OnHide(c);
}

UIElement* UICreateElement(char* ID, UIElement* parent, bool clickable, Vector2 min, Vector2 max, Vector2 anchorMin, Vector2 anchorMax, bool anchorOnlyOrigin, bool isMainPanel)
{
	UIElement e = (UIElement){
		.Parent = parent,
		.IsValid = true,
		.IsVisible = true,
		.IsOpen = false,
		.Clickable = clickable,
		.OnDraw = NULL,
		.IsMainPanel = isMainPanel
	};

	strcpy_s(e.ID, 32, ID);
	e.Hash = ToHash(e.ID);

	if (e.Parent == NULL)
		e.ParentRect = UICreateRect(0, 0, GetScreenWidth(), GetScreenHeight());
	else
		e.ParentRect = e.Parent->Rect;

	float x1, y1, x2, y2;
	//anchormin = parent center to parent min
	//anchormax = parent center to parent max

	if (anchorOnlyOrigin)
	{
		//anchor only center using anchorMin, uses max as width and height instead

		Vector2 half = Vector2Scale(max, 0.5);
		anchorMin.x = Remap(anchorMin.x, -1, 1, 0, 1);
		anchorMin.y = Remap(anchorMin.y, -1, 1, 0, 1);

		x1 = Lerp(e.ParentRect.Min.x, e.ParentRect.Max.x, anchorMin.x) + min.x;
		y1 = Lerp(e.ParentRect.Min.y, e.ParentRect.Max.y, anchorMin.y) + min.y;

		x1 -= half.x;
		y1 -= half.y;
		x2 = max.x;
		y2 = max.y;

		e.Rect = UICreateRect(x1, y1, x1 + x2, y1 + y2);
	}
	else
	{
		//anchor all 4 sides according to parent, uses min and max as absolute positions

		x1 = Lerp(e.ParentRect.Position.x, e.ParentRect.Min.x, anchorMin.x) + min.x;
		y1 = Lerp(e.ParentRect.Position.y, e.ParentRect.Min.y, anchorMin.y) + min.y;
		x2 = Lerp(e.ParentRect.Position.x, e.ParentRect.Max.x, anchorMax.x) - max.x;
		y2 = Lerp(e.ParentRect.Position.y, e.ParentRect.Max.y, anchorMax.y) - max.y;

		e.Rect = UICreateRect(x1, y1, x2, y2);
	}

	UIElementList[UINextElementSlot] = e;
	UINextElementSlot++;

	return &UIElementList[UINextElementSlot-1];
}

Rect UICreateRect(float x1, float y1, float x2, float y2)
{
	Rect r = {
		.Min = (Vector2){x1, y1},
		.Max = (Vector2){x2, y2},
	};

	r.Rectangle = (Rectangle)
	{
		.x = x1,
		.y = y1,
		.width = fabsf(x2 - x1),
		.height = fabsf(y2 - y1)
	};

	r.Position = (Vector2){
		.x = (x1+x2)/2,
		.y = (y1+y2)/2
	};

	return r;
}


UIElement* UIFindElement(char* ID)
{
	for (int i = 0; i < UINextElementSlot; i++)
	{
		UIElement* c = &UIElementList[i];
		if (strcmp(ID, c->ID) == 0)
			return c;
	}

	return NULL;
}