#include <raylib.h>
#include <raymath.h>
#include "projectile.h"

int ProjectileCount = 64;
int NextProjectileIndex = 0;


void ProjectileSpawn(Vector2 from, Vector2 dir, float speed, float damage)
{

}

void ProjectileUpdate()
{
	for (int i = 0; i < ProjectileCount; i++)
	{
		Projectile* p = ProjectileList[i];
		if (p == NULL || !p->_isAlive)
			continue;


	}
}


void ProjectileDraw()
{
	for (int i = 0; i < ProjectileCount; i++)
	{
		Projectile* p = ProjectileList[i];
		if (p == NULL || !p->_isAlive)
			continue;


	}
}