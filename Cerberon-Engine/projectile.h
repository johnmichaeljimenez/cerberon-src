#pragma once

typedef struct Projectile
{
	Vector2 From;
	Vector2 Direction;
	float Speed;
	float Damage;

	bool _hit;
	Vector2 _hitPos;
} Projectile;

Projectile* ProjectileList[64];