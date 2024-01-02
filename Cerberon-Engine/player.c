#include <raylib.h>
#include <raymath.h>
#include "asset_manager.h"
#include "player.h"
#include "input_handler.h"

void PlayerInit(PlayerCharacter* p)
{
	p->Position = Vector2Zero();
	p->Rotation = 0;
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
}

void PlayerLateUpdate(PlayerCharacter* p)
{

}

void PlayerDraw(PlayerCharacter* p)
{
	DrawCircleLines(p->Position.x, p->Position.y, p->CollisionRadius, GREEN);
	DrawLineV(p->Position, Vector2Add(p->Position, Vector2Scale(p->Direction, p->CollisionRadius)), GREEN);
}

void PlayerDrawHUD(PlayerCharacter* p)
{

}