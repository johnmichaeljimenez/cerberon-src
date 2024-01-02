#pragma once
#include <raylib.h>
#include <raymath.h>
#include "asset_manager.h"

typedef struct PlayerCharacter
{
	Vector2 Position;
	float Rotation;
	Vector2 Direction;

	float MovementSpeed;
	float CollisionRadius;

} PlayerCharacter;

PlayerCharacter PlayerEntity;

void PlayerInit(PlayerCharacter* p);
void PlayerUnload(PlayerCharacter* p);
void PlayerUpdate(PlayerCharacter* p);
void PlayerLateUpdate(PlayerCharacter* p);
void PlayerDraw(PlayerCharacter* p);
void PlayerDrawHUD(PlayerCharacter* p);