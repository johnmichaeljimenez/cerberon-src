#include <raylib.h>
#include <raymath.h>
#include "camera.h"


void CameraInit()
{
	GameCamera = (Camera2D) {
		.offset = (Vector2){ GetScreenWidth() / 2, GetScreenHeight() / 2 },
		.zoom = 1,
		.target = Vector2Zero()
	};
}

void CameraUpdate()
{
	GameCamera.target = Vector2Lerp(GameCamera.target, CameraTargetPosition, GetFrameTime());
}

void CameraSetTarget(Vector2 pos, bool jump)
{
	CameraTargetPosition = pos;

	if (jump)
		GameCamera.target = pos;
}

Vector2 CameraGetMousePosition()
{
	return GetScreenToWorld2D(GetMousePosition(), GameCamera);
}