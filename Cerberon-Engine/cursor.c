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
	DrawRectangleV(curPos, (Vector2) { 32, 32 }, WHITE);
}