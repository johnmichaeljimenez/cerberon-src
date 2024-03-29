#pragma once

typedef struct Rect
{
	Vector2 Min;
	Vector2 Max;
	Rectangle Rectangle;
	Vector2 Position;
} Rect;

typedef struct UIElement
{
	struct UIElement* Parent;
	struct UIElement* Children[64];

	char* ID[32];
	unsigned long Hash;

	Rect Rect;
	Rect ParentRect;

	bool IsValid;
	bool IsVisible;
	bool IsOpen;

	bool Clickable;
	bool Selected;
	bool Hovered;

	bool IsMainPanel;

	void(*OnDeselect)(struct UIElement* c);
	void(*OnSelect)(struct UIElement* c);
	void(*OnClick)(struct UIElement* c);
	void(*OnDraw)(struct UIElement* c);
	void(*OnShow)(struct UIElement* c);
	void(*OnHide)(struct UIElement* c);
	void(*OnUpdate)(struct UIElement* c);
} UIElement;

UIElement* UICurrentElement;
UIElement UIElementList[1024];
int UINextElementSlot;
UIElement* UICurrentSelected;
UIElement* UICurrentHovered;

bool UIIsVisible; //dialogues, popups, etc

void UILoadData();
void UIInit();
void UISetSelection(UIElement* c);
void UIUpdate();
void UIDraw();
void UIShow(UIElement* c);
void UIHide(UIElement* c);

Rect UICreateRect(float x1, float y1, float x2, float y2);

UIElement* UICreateElement(char* ID, UIElement* parent, bool clickable, Vector2 min, Vector2 max, Vector2 anchorMin, Vector2 anchorMax, bool anchorOnlyOrigin, bool isMainPanel);
UIElement* UIFindElement(char* ID);