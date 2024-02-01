#pragma once

typedef struct UIControl
{
	void(*OnDeselect)(struct UIControl* c);
	void(*OnSelect)(struct UIControl* c);
	void(*OnDraw)(void* data);
} UIControl;

UIControl* UICurrentSelected;
bool UIIsVisible; //dialogues, popups, etc
void UIInit();
void UISetSelection(UIControl* c);
void UIUpdate();
void UIDraw();