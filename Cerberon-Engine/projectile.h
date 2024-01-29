#pragma once

typedef struct Projectile
{
	Vector2 From;
	Vector2 Direction;
	float Speed;
	float Damage;

	bool _isAlive;
	bool _hit;
	Vector2 _hitPos;
} Projectile;

Projectile* ProjectileList[64];
void ProjectileSpawn(Vector2 from, Vector2 dir, float speed, float damage);
void ProjectileUpdate();
void ProjectileDraw();