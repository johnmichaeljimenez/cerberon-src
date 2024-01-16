#pragma once
#include <raylib.h>

typedef struct Trigger
{
	Vector2 Position;
	float Rotation;
	Vector2 Size;

	char* Target[32];
	float Delay;
	float OneShot;
	float Cooldown;

} Trigger;