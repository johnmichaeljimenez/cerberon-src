#include <raylib.h>
#include <raymath.h>
#include "projectile.h"
#include "collision.h"
#include "time.h"
#include "utils.h"

int ProjectileCount = 64;
int NextProjectileIndex = 0;

static float flashDuration = 0.3;
static TextureResource* bulletTracerSprite;

void ProjectileInit()
{
	bulletTracerSprite = GetTextureResource(ToHash("misc-bullet-tracer"));

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
			._position = Vector2Zero(),
			._flashTime = 0
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
	p._flashTime = flashDuration;

	p.Rotation = atan2f(dir.y, dir.x);
	//TraceLog(LOG_INFO, "%f", p.Rotation);

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
	BeginBlendMode(BLEND_ADDITIVE);

	for (int i = 0; i < ProjectileCount; i++)
	{
		Projectile p = ProjectileList[i];
		if (!p._isAlive)
			continue;

		if (Vector2LengthSqr(p.From, p._position) > (128 * 128))
			continue;
		
		DrawSprite(bulletTracerSprite, p._position, p.Rotation, 0.4, (Vector2) { 0.3, 0 }, WHITE);
	}

	EndBlendMode();
}


void ProjectileDrawLights()
{
	BeginBlendMode(BLEND_ADDITIVE);

	for (int i = 0; i < ProjectileCount; i++)
	{
		Projectile* p = &ProjectileList[i];

		if (Vector2LengthSqr(p->From, p->_position) > (128 * 128))
		{
			if (p->_isAlive)
			{
				DrawSprite(bulletTracerSprite, p->_position, p->Rotation, 0.4, (Vector2) { 0.3, 0 }, GRAY);
			}
		}

		if (p->_flashTime <= 0)
			continue;
		p->_flashTime -= TICKRATE;

		DrawCircleGradient(p->From.x, p->From.y, 256, ColorBrightness01(WHITE, p->_flashTime/ flashDuration), BLACK);
	}

	EndBlendMode();
}