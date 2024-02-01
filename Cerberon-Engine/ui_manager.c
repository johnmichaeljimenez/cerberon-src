#include <raylib.h>
#include <raymath.h>
#include "ui_manager.h"

void UIInit()
{

}

void UISetSelection(UIControl* c)
{
	if (c->OnDeselect != NULL)
		c->OnDeselect(c);

	UICurrentSelected = c;

	if (c != NULL && c->OnSelect != NULL)
		c->OnSelect(c);
}

void UIUpdate()
{

}

void UIDraw()
{

}