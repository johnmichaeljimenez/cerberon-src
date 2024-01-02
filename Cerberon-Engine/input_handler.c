#include <raylib.h>
#include <raymath.h>
#include "input_handler.h"

Vector2 GetInputMovement()
{
	Vector2 m = Vector2Zero();

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