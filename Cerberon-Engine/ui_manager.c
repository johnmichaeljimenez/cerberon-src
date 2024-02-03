#include <raylib.h>
#include <raymath.h>
#include "ui_manager.h"
#include "u_dialogue.h"

void UIInit()
{
	UINextElementSlot = 0;
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

		if (!c->IsValid || !c->IsVisible || !c->Clickable)
			continue;

		if (CheckCollisionPointRec(mousePos, c->Rect))
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

		if (!c->IsValid || !c->IsVisible)
			continue;

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

		if (c->IsVisible)
			return true;
	}

	return false;
}

void UIShow(UIElement* c)
{
	UIIsVisible = IsAnyUIVisible();
	if (!UIIsVisible)
		return;

	c->IsOpen = true;

	if (c->OnShow != NULL)
		c->OnShow(c);
}

void UIHide(UIElement* c)
{
	UIIsVisible = IsAnyUIVisible();
	if (!UIIsVisible)
		return;

	c->IsOpen = false;

	if (c->OnHide != NULL)
		c->OnHide(c);
}

UIElement UICreateElement(UIElement* parent, bool clickable)
{
	UIElement e = (UIElement){
		.Parent = parent,
		.IsValid = true,
		.IsVisible = true,
		.IsOpen = false,
	};

	if (e.Parent == NULL)
		e.ParentRect = (Rectangle){ 0, 0, GetScreenWidth(), GetScreenHeight() };
	else
		e.ParentRect = e.Parent->Rect;

	return e;
}

void UICalculateRect(UIElement* e, float x1, float y1, float x2, float y2)
{
	e->_inRectMin = (Vector2){ x1, y1 },
	e->_inRectMax = (Vector2){ x2, y2 },

	e->Rect = (Rectangle){
		.x = e->_inRectMin.x,
		.y = e->_inRectMin.y,
		.width = (e->_inRectMax.x - e->_inRectMin.x),
		.height = (e->_inRectMax.y - e->_inRectMin.y)
	};
}