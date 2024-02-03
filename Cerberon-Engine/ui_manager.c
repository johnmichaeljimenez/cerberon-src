#include <raylib.h>
#include <raymath.h>
#include "ui_manager.h"
#include "u_dialogue.h"

void UIInit()
{
	UIPanel p = UICreatePanel(UDialogueShow, UDialogueHide, UDialogueDraw);

	UIPanelList[0] = p;
}

void UISetSelection(UIControl* c)
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
	UIControl* newHovered = NULL;
	Vector2 mousePos = GetMousePosition();

	for (int i = 0; i < 32; i++)
	{
		UIPanel* p = &UIPanelList[i];
		if (!p->IsValid || !p->IsVisible || !p->IsOpen)
			continue;

		for (int j = 0; j < 32; j++)
		{
			UIControl* c = p->Controls[j];
			if (c == NULL)
				continue;

			if (!c->IsValid || !c->IsVisible || !c->Clickable)
				continue;

			if (CheckCollisionPointRec(mousePos, c->Rect))
			{
				newHovered = c;
			}
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
		UIPanel* p = &UIPanelList[0];
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
		UIPanel* p = &UIPanelList[i];
		if (!p->IsValid || !p->IsVisible || !p->IsOpen)
			continue;

		if (p->OnDraw != NULL)
			p->OnDraw(p);

		for (int j = 0; j < 32; j++)
		{
			UIControl* c = p->Controls[j];
			if (c == NULL)
				continue;

			if (!c->IsValid || !c->IsVisible)
				continue;

			c->OnDraw(c);
		}
	}
}

void UIShow(UIPanel* c)
{
	UIIsVisible = true;
	c->IsOpen = true;

	if (c->OnShow != NULL)
		c->OnShow(c);
}

void UIHide(UIPanel* c)
{
	UIIsVisible = false;
	c->IsOpen = false;

	if (c->OnHide != NULL)
		c->OnHide(c);
}

UIPanel UICreatePanel(void(*onShow)(UIPanel* p), void(*onHide)(UIPanel* p), void(*onDraw)(UIPanel* p))
{
	return (UIPanel) {
		.IsValid = true,
		.IsVisible = true,
		.IsOpen = false,
		.OnShow = onShow,
		.OnHide = onHide,
		.OnDraw = onDraw
	};
}

UIControl UICreateControl(UIPanel* parent, Rectangle rect, void(*onDraw)(UIControl *c))
{
	return (UIControl) {
		.Clickable = true,
		.Hovered = false,
		.IsValid = true,
		.Parent = parent,
		.Rect = rect,
		.Selected = false,
		.OnDraw = onDraw
	};
}