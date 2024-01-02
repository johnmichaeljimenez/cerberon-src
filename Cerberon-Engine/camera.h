#pragma once
#include <raylib.h>
#include <raymath.h>

Camera2D GameCamera;
Vector2 CameraTargetPosition;

void CameraInit();
void CameraUpdate();
void CameraSetTarget(Vector2 pos, bool jump);
Vector2 CameraGetMousePosition();