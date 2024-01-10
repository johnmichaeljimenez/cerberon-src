#pragma once
#include <raylib.h>
#include <raymath.h>

typedef enum CursorStates
{
	CURSORSTATE_None,
	CURSORSTATE_Menu,
	CURSORSTATE_IngameInteractReticle,
	CURSORSTATE_IngameInteractHover,
	CURSORSTATE_IngameInteractEnabled,
} CursorStates;

void CursorInit();
void CursorChange(CursorStates c);
void CursorUpdate();
void CursorDraw();
void CursorOverridePosition(Vector2 pos);