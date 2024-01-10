#include <raylib.h>
#include <raymath.h>
#include "cursor.h"

static Vector2 curPos;
static CursorStates cursorState;

void CursorInit()
{
	HideCursor();
}

void CursorChange(CursorStates c)
{
	cursorState = c;
}

void CursorDraw()
{
	curPos = GetMousePosition();

	switch (cursorState)
	{
	case CURSORSTATE_None:
		break;

	case CURSORSTATE_Menu:
		DrawCircleV(curPos, 2, WHITE);
		break;

	case CURSORSTATE_IngameInteractReticle:
		DrawCircleV(curPos, 2, DARKGRAY);
		DrawCircleLines(curPos.x, curPos.y, 32, LIGHTGRAY);
		break;

	case CURSORSTATE_IngameInteractHover:
		DrawCircleV(curPos, 2, DARKGRAY);
		DrawRectangle(curPos.x - 8, curPos.y - 8, 16, 16, DARKGRAY);
		break;

	case CURSORSTATE_IngameInteractEnabled:
		DrawCircleV(curPos, 2, DARKGRAY);
		DrawRectangle(curPos.x - 8, curPos.y - 8, 16, 16, WHITE);
		break;

	default:
		break;
	}
}