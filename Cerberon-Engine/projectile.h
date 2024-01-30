#pragma once

typedef struct Projectile
{
	Vector2 From;
	Vector2 Direction;
	float Rotation;
	float Speed;
	float Damage;

	Vector2 _position;
	float _lifeTime;
	bool _isAlive;
	bool _hit;
	Vector2 _hitPos;
} Projectile;

Projectile ProjectileList[64];
void ProjectileInit();
void ProjectileSpawn(Vector2 from, Vector2 dir, float speed, float damage);
void ProjectileUpdate();
void ProjectileDraw();
void ProjectileDrawLights();