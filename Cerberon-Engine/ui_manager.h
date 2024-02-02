#pragma once

typedef struct UIControl
{
	Rectangle Rect;

	bool IsVisible;
	bool IsValid;
	bool Clickable;
	bool Selected;
	bool Hovered;
	struct UIPanel* Parent;

	void(*OnDeselect)(struct UIControl* c);
	void(*OnSelect)(struct UIControl* c);
	void(*OnClick)(struct UIControl* c);
	void(*OnDraw)(struct UIControl* c);
} UIControl;

typedef struct UIPanel
{
	UIControl* Controls[32];
	bool IsVisible;
	bool IsValid;

	void (*OnShow)(UIControl* c);
	void (*OnHide)(UIControl* c);
} UIPanel;

UIPanel* UICurrentPanel;
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

UIPanel UICreatePanel(void(*onShow)(UIPanel* p), void(*onHide)(UIPanel* p));
UIControl UICreateControl(UIPanel* parent, Rectangle rect, void(*onDraw)(UIControl* c));