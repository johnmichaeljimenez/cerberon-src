#pragma once

typedef struct UIElement
{
	struct UIElement* Parent;
	struct UIElement* Children[64];

	Rectangle InRect;
	Rectangle OutRect;

	bool IsValid;
	bool IsVisible;
	bool IsOpen;

	bool Clickable;
	bool Selected;
	bool Hovered;

	void(*OnDeselect)(struct UIElement* c);
	void(*OnSelect)(struct UIElement* c);
	void(*OnClick)(struct UIElement* c);
	void(*OnDraw)(struct UIElement* c);
	void(*OnShow)(struct UIElement* c);
	void(*OnHide)(struct UIElement* c);
} UIElement;

UIElement* UICurrentElement;
UIElement UIElementList[1024];
UIElement* UICurrentSelected;
UIElement* UICurrentHovered;

bool UIIsVisible; //dialogues, popups, etc
void UIInit();
void UISetSelection(UIElement* c);
void UIUpdate();
void UIDraw();
void UIShow(UIElement* c);
void UIHide(UIElement* c);

UIElement UICreateElement(void(*onShow)(UIElement* p), void(*onHide)(UIElement* p), void(*onDraw)(UIElement* p));