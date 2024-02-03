#include <raylib.h>
#include <raymath.h>
#include "ui_manager.h"
#include "u_dialogue.h"

void UIInit()
{
	UIElement p = UICreateElement(UDialogueShow, UDialogueHide, UDialogueDraw);

	UIElementList[0] = p;
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

	for (int i = 0; i < 32; i++)
	{
		UIElement* c = &UIElementList[i];
		if (c == NULL)
			continue;

		if (!c->IsValid || !c->IsVisible || !c->Clickable)
			continue;

		if (CheckCollisionPointRec(mousePos, c->OutRect))
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
	for (int i = 0; i < 32; i++)
	{
		UIElement* c = &UIElementList[i];
		if (c == NULL)
			continue;

		if (!c->IsValid || !c->IsVisible)
			continue;

		c->OnDraw(c);
	}
}

void UIShow(UIElement* c)
{
	UIIsVisible = true;
	c->IsOpen = true;

	if (c->OnShow != NULL)
		c->OnShow(c);
}

void UIHide(UIElement* c)
{
	UIIsVisible = false;
	c->IsOpen = false;

	if (c->OnHide != NULL)
		c->OnHide(c);
}

UIElement UICreateElement(void(*onShow)(UIElement* p), void(*onHide)(UIElement* p), void(*onDraw)(UIElement* p))
{
	return (UIElement) {
		.IsValid = true,
		.IsVisible = true,
		.IsOpen = false,
		.OnShow = onShow,
		.OnHide = onHide,
		.OnDraw = onDraw
	};
}