#pragma once
#include <raylib.h>
#include <raymath.h>

typedef enum InputActionType
{
	INPUTACTIONTYPE_UIConfirm,
	INPUTACTIONTYPE_UIBack,
	INPUTACTIONTYPE_Interact,
	INPUTACTIONTYPE_Flashlight,
} InputActionType;

typedef struct InputAction
{
	InputActionType Type;
	int Key;
	float Time;
	bool Released;
	bool IsMouse;
} InputAction;

bool InputGetKeyPressed(KeyboardKey key);
bool InputGetMousePressed(MouseButton button);
bool InputGetMouseDown(MouseButton button);

void InputUpdate();
bool InputGetPressed(InputActionType type);
Vector2 InputGetMovement();
bool InputGetMouseDirection(Vector2 from, Vector2* out);