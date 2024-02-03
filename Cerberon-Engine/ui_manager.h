#pragma once

typedef struct UIElement
{
	struct UIElement* Parent;
	struct UIElement* Children[64];

	Vector2 _inRectMin;
	Vector2 _inRectMax;
	Rectangle Rect;
	Rectangle ParentRect;

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
int UINextElementSlot;
UIElement* UICurrentSelected;
UIElement* UICurrentHovered;

bool UIIsVisible; //dialogues, popups, etc
void UIInit();
void UISetSelection(UIElement* c);
void UIUpdate();
void UIDraw();
void UIShow(UIElement* c);
void UIHide(UIElement* c);

UIElement UICreateElement(UIElement* parent, bool clickable);
void UICalculateRect(UIElement* e, float x1, float y1, float x2, float y2);