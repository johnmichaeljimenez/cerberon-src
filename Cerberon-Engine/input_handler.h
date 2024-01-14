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

void InputUpdate();
bool InputGetPressed(InputActionType type);
Vector2 InputGetMovement();