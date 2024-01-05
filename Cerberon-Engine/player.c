#include <raylib.h>
#include <raymath.h>
#include "asset_manager.h"
#include "player.h"
#include "camera.h"
#include "input_handler.h"
#include "utils.h"
#include "mapdata_manager.h"
#include "collision.h"

static unsigned long hash;
static LinecastHit lineHit;

void PlayerInit(PlayerCharacter* p)
{
	hash = ToHash("survivor-idle_shotgun_0");

	p->Position = CurrentMapData->PlayerPosition;
	p->Rotation = CurrentMapData->PlayerRotation;
	p->Direction = (Vector2){ 1, 0 };
	p->CollisionRadius = 32;
	p->MovementSpeed = 200;
	p->CameraOffset = 300;
}

void PlayerUnload(PlayerCharacter* p)
{

}

Vector2 PlayerGetForward(PlayerCharacter* p, float length)
{
	return Vector2Add(p->Position, Vector2Scale(p->Direction, length));
}

void PlayerUpdate(PlayerCharacter* p)
{
	Vector2 vel = Vector2Scale(GetInputMovement(), p->MovementSpeed);
	vel = Vector2Scale(vel, GetFrameTime());
	p->Position = Vector2Add(p->Position, vel);

	//collision here
	MoveBody(&p->Position, p->CollisionRadius);

	Vector2 diff = CameraGetMousePosition();
	diff = Vector2Subtract(diff, p->Position);
	float newDir = atan2f(diff.y, diff.x);

	Linecast(p->Position, PlayerGetForward(p, 1300), &lineHit);

	PlayerRotate(p, newDir);
}

void PlayerLateUpdate(PlayerCharacter* p)
{
	Vector2 targPos = Vector2Subtract(CameraGetMousePosition(), p->Position);
	targPos = Vector2Add(p->Position, Vector2ClampValue(targPos, 0, 300));

	CameraSetTarget(targPos, false);
}

void PlayerDraw(PlayerCharacter* p)
{
	TextureResource* t = GetTextureResource(hash);

	if (t != NULL)
	{
		DrawSprite(t, p->Position, p->Rotation, 0.6, (Vector2) { -0.15, 0.1 });
	}

	if (lineHit.WallHit != NULL)
	{
		DrawLineV(lineHit.From, lineHit.To, RED);
		DrawCircleV(lineHit.To, 3, RED);
	}

	DrawCircleLines(p->Position.x, p->Position.y, p->CollisionRadius, GREEN);
	DrawLineV(p->Position, PlayerGetForward(p, p->CollisionRadius), GREEN);
}

void PlayerDrawHUD(PlayerCharacter* p)
{

}


void PlayerRotate(PlayerCharacter* p, float dir)
{
	p->Rotation = dir;
	p->Direction.x = cosf(p->Rotation);
	p->Direction.y = sinf(p->Rotation);
}