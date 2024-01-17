#include <raylib.h>
#include <raymath.h>
#include "camera.h"
#include "time.h"

static void UpdateCameraViewBounds()
{
	Vector2 size = (Vector2){ GetScreenWidth(), GetScreenHeight() };
	Vector2 from, to;
	from = GetScreenToWorld2D((Vector2) { 0, 0 }, GameCamera);
	to = GetScreenToWorld2D(size, GameCamera);

	CameraViewBounds.x = from.x;
	CameraViewBounds.y = from.y;

	CameraViewBounds.width = size.x;
	CameraViewBounds.height = size.y;
}

void CameraInit()
{
	GameCamera = (Camera2D) {
		.offset = (Vector2){ GetScreenWidth() / 2, GetScreenHeight() / 2 },
		.zoom = 1,
		.target = Vector2Zero()
	};
	UpdateCameraViewBounds();
}

void CameraUpdate()
{
	GameCamera.target = Vector2Lerp(GameCamera.target, CameraTargetPosition, TICKRATE * 2);
	UpdateCameraViewBounds();
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