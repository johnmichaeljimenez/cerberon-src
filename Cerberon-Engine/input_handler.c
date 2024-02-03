#include <raylib.h>
#include <raymath.h>
#include "input_handler.h"
#include "ui_manager.h"
#include "camera.h"
#include "time.h"

static InputAction Actions[] = {
	{ INPUTACTIONTYPE_UIConfirm, KEY_SPACE, 0, false, false },
	{ INPUTACTIONTYPE_UIBack, KEY_F1, 0, false, false },
	{ INPUTACTIONTYPE_Interact, MOUSE_BUTTON_LEFT, 0, false, true },
	{ INPUTACTIONTYPE_Flashlight, KEY_F, 0, false, false },
};

static void DoTimer(float* t)
{
	if (*t > 0)
	{
		*t -= GetFrameTime();
		if (*t <= 0)
			*t = 0;
	}
}

bool InputGetKeyPressed(KeyboardKey key)
{
	if (UIIsVisible)
		return false;

	return IsKeyPressed(key);
}

bool InputGetMousePressed(MouseButton button)
{
	if (UIIsVisible)
		return false;

	return IsMouseButtonPressed(button);
}

bool InputGetMouseDown(MouseButton button)
{
	if (UIIsVisible)
		return false;

	return IsMouseButtonDown(button);
}

static void CheckInput(InputAction* i)
{
	if (i->Released)
	{
		if (i->IsMouse)
		{
			if (IsMouseButtonReleased(i->Key))
			{
				i->Released = false;
				i->Time = 0;
			}
		}
		else
		{
			if (IsKeyUp(i->Key))
			{
				i->Released = false;
				i->Time = 0;
			}
		}
	}
	else
	//if (!i->Released)
	{
		if (i->IsMouse)
		{
			if (IsMouseButtonDown(i->Key))
				i->Time = 0.2f;
		}
		else
		{
			if (IsKeyDown(i->Key))
				i->Time = 0.2;
		}
	}

	DoTimer(&(i->Time));
}

void InputUpdate()
{
	/*for (int i = 0; i < 4; i++)
	{
		CheckInput(&(Actions[i]));
	}*/
}

bool InputGetPressed(InputActionType type)
{
	if (UIIsVisible)
		return false;

	InputAction action = Actions[type];
	return action.IsMouse? IsMouseButtonPressed(action.Key) : InputGetKeyPressed(action.Key);

	/*bool pressed = action.Time > 0 && !action.Released;

	if (pressed) {
		action.Time = 0;
		Actions[type].Released = true;
	}

	return pressed;*/
}

Vector2 InputGetMovement()
{
	Vector2 m = Vector2Zero();

	if (UIIsVisible)
		return m;

	if (IsKeyDown(KEY_W))
		m.y = -1;
	else if (IsKeyDown(KEY_S))
		m.y = 1;

	if (IsKeyDown(KEY_A))
		m.x = -1;
	else if (IsKeyDown(KEY_D))
		m.x = 1;

	m = Vector2Normalize(m);

	return m;
}

bool InputGetMouseDirection(Vector2 from, Vector2* out)
{
	if (UIIsVisible)
		return false;

	Vector2 diff = CameraGetMousePosition();
	*out = Vector2Subtract(diff, from);

	return true;
}