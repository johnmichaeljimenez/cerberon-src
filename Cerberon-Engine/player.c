#include <raylib.h>
#include <raymath.h>
#include "asset_manager.h"
#include "player.h"
#include "camera.h"
#include "input_handler.h"
#include "utils.h"
#include "mapdata_manager.h"

static unsigned long hash;

void PlayerInit(PlayerCharacter* p)
{
	hash = ToHash("survivor-idle_shotgun_0");

	p->Position = CurrentMapData->PlayerPosition;
	p->Rotation = CurrentMapData->PlayerRotation;
	p->Direction = (Vector2){ 1, 0 };
	p->CollisionRadius = 28;
	p->MovementSpeed = 200;
}

void PlayerUnload(PlayerCharacter* p)
{

}

void PlayerUpdate(PlayerCharacter* p)
{
	Vector2 vel = Vector2Scale(GetInputMovement(), p->MovementSpeed);
	vel = Vector2Scale(vel, GetFrameTime());
	p->Position = Vector2Add(p->Position, vel);

	//collision here

	Vector2 diff = CameraGetMousePosition();
	diff = Vector2Subtract(diff, p->Position);
	float newDir = atan2f(diff.y, diff.x);

	PlayerRotate(p, newDir);
}

void PlayerLateUpdate(PlayerCharacter* p)
{

}

void PlayerDraw(PlayerCharacter* p)
{
	TextureResource* t = GetTextureResource(hash);

	if (t != NULL)
	{
		DrawSprite(t, p->Position, p->Rotation, 0.6);
	}

	DrawCircleLines(p->Position.x, p->Position.y, p->CollisionRadius, GREEN);
	DrawLineV(p->Position, Vector2Add(p->Position, Vector2Scale(p->Direction, p->CollisionRadius)), GREEN);
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