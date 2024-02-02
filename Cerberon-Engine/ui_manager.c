#include <raylib.h>
#include <raymath.h>
#include "ui_manager.h"

void UIInit()
{

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

}

void UIDraw()
{

}

void UIShow(UIPanel* c)
{

}

void UIHide(UIPanel* c)
{

}