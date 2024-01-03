#pragma once
#include <raylib.h>
#include <raymath.h>

typedef struct Wall
{
	Vector2 From;
	Vector2 To;
	float Length;
	Vector2 Normal;
	Vector2 Midpoint;
};

void LoadMap(char* filename);