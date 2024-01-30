#include <raylib.h>
#include <raymath.h>
#include "projectile.h"
#include "collision.h"
#include "time.h"

int ProjectileCount = 64;
int NextProjectileIndex = 0;

void ProjectileInit()
{
	for (int i = 0; i < ProjectileCount; i++)
	{
		Projectile p = (Projectile){
			.Damage = 0,
			.Direction = Vector2Zero(),
			.From = Vector2Zero(),
			.Speed = 0,
			._hit = false,
			._hitPos = Vector2Zero(),
			._isAlive = false,
			._lifeTime = 0,
			._position = Vector2Zero()
		};

		ProjectileList[i] = p;
	}
}

void ProjectileSpawn(Vector2 from, Vector2 dir, float speed, float damage)
{
	Projectile p = { 0 };

	p._lifeTime = 5.0f;
	p._isAlive = true;
	p._position = from;
	p._hit = false;
	p.From = from;
	p.Direction = dir;
	p.Speed = speed;
	p.Damage = damage;

	ProjectileList[NextProjectileIndex] = p;
	NextProjectileIndex++;
	if (NextProjectileIndex >= ProjectileCount)
		NextProjectileIndex = 0;
}

void ProjectileUpdate()
{
	for (int i = 0; i < ProjectileCount; i++)
	{
		Projectile* p = &ProjectileList[i];
		if (!p->_isAlive)
			continue;

		p->_lifeTime -= TICKRATE;
		if (p->_lifeTime <= 0)
		{
			p->_lifeTime = 0;
			p->_isAlive = false;
			continue;
		}

		LinecastHit l;
		Vector2 to = Vector2Add(p->_position, Vector2Scale(p->Direction, p->Speed * TICKRATE));

		if (Linecast(p->_position, to, &l))
		{
			//TraceLog(LOG_INFO, "HIT! %f -> %f, %f", l.Length, l.To.x, l.To.y);
			to = l.To;
			p->_hitPos = l.To;
			p->_lifeTime = 0;
			p->_isAlive = false;
			p->_position = to;
			continue;
		}

		p->_position = to;
	}
}


void ProjectileDraw()
{
	for (int i = 0; i < ProjectileCount; i++)
	{
		Projectile p = ProjectileList[i];
		if (!p._isAlive)
			continue;

		DrawCircleV(p._position, 12, RED);
	}
}