#pragma once
#include <raylib.h>

typedef struct Hitbox
{
	Rectangle Bounds;
	int Faction; //0 - player
} Hitbox;

typedef struct Hurtbox
{
	Rectangle Bounds;
	int Faction; //0 - player
	bool IsEnabled;
	void (*OnHit)(struct Hurtbox* hurtbox, struct Hitbox* hitbox); //actual hit effects (ex. hp)
} Hurtbox;

bool CheckHitboxHit(Hitbox* hitbox, int hurtboxCount, Hurtbox* hurtboxList);