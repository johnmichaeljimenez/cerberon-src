#pragma once
#include <raylib.h>
#include <raymath.h>
#include "mapdata_manager.h"

void MoveBody(Vector2* pos, float radius);
bool GetLineIntersection(float p0_x, float p0_y, float p1_x, float p1_y,
	float p2_x, float p2_y, float p3_x, float p3_y, Vector2* hit);
Vector2 WallGetClosestPoint(Vector2 p1, Vector2 p2, Vector2 pos);