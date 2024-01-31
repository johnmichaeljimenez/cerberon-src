#pragma once

typedef struct Projectile
{
	Vector2 From;
	Vector2 Direction;
	float Rotation;
	float Speed;
	float Damage;

	float _flashTime;
	Vector2 _position;
	float _lifeTime;
	bool _isAlive;
	bool _hit;
	Vector2 _hitPos;
	int _height;
} Projectile;

Projectile ProjectileList[64];
void ProjectileInit();
void ProjectileSpawn(Vector2 from, Vector2 dir, float speed, float damage, int height);
void ProjectileUpdate();
void ProjectileDraw();
void ProjectileDrawLights();