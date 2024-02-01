#pragma once

typedef struct UIControl
{
	Rectangle Rect;

	bool Clickable;
	bool Selected;
	struct UIPanel* Parent;

	void(*OnDeselect)(struct UIControl* c);
	void(*OnSelect)(struct UIControl* c);
	void(*OnClick)(struct UIControl* c);
	void(*OnDraw)(void* data);
} UIControl;

typedef struct UIPanel
{
	UIControl* Controls[32];
	bool Visible;

	void (*OnShow)(UIControl* c);
	void (*OnHide)(UIControl* c);
} UIPanel;

UIPanel UIPanelList[32];
UIControl* UICurrentSelected;
UIControl* UICurrentHovered;

bool UIIsVisible; //dialogues, popups, etc
void UIInit();
void UISetSelection(UIControl* c);
void UIUpdate();
void UIDraw();
void UIShow(UIPanel* c);
void UIHide(UIPanel* c);