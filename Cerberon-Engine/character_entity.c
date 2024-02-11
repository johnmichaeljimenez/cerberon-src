#include <raylib.h>
#include "character_entity.h"
#include "memory.h"
#include <raymath.h>
#include "player.h"

void CharacterInit()
{
	CharacterEntityListSize = 64;
	CharacterEntityCount = 0;
	CharacterEntityList = MCalloc(CharacterEntityListSize, sizeof(CharacterEntity), "Character List");

	PlayerEntity = CharacterSpawn(Vector2Zero(), 0, 64, 100, &PlayerData, PlayerInit, PlayerUpdate, PlayerLateUpdate, NULL);
}

void CharacterUnload()
{
	CharacterOnDespawn(PlayerEntity);
	MFree(CharacterEntityList, CharacterEntityListSize, sizeof(CharacterEntity), "Character List");
}

void CharacterUpdate()
{
	for (int i = 0; i < CharacterEntityCount; i++)
	{
		CharacterEntity* c = &CharacterEntityList[i];
		if (!c->IsValid)
			continue;

		if (c->OnUpdate != NULL)
			c->OnUpdate(c);
	}

	for (int i = 0; i < CharacterEntityCount; i++)
	{
		CharacterEntity* c = &CharacterEntityList[i];
		if (!c->IsValid)
			continue;

		if (c->OnLateUpdate != NULL)
			c->OnLateUpdate(c);
	}
}

CharacterEntity* CharacterSpawn(Vector2 pos, float rot, float radius, int hp, void* data, void(*onSpawn)(CharacterEntity* c), void(*onUpdate)(CharacterEntity* c), void(*onLateUpdate)(CharacterEntity* c), void(*onDeath)(CharacterEntity* c))
{
	CharacterEntity c = { 0 };
	c.Position = pos;
	c.Rotation = rot;
	c.IsDead = false;
	c.ColliderRadius = radius;

	c.OnSpawn = onSpawn;
	c.OnUpdate = onUpdate;
	c.OnLateUpdate = onLateUpdate;
	c.OnDeath = onDeath;

	c.Index = CharacterEntityCount;
	c.IsValid = true;

	CharacterEntityList[CharacterEntityCount] = c;
	CharacterEntity* i = &CharacterEntityList[CharacterEntityCount];
	if (i->OnSpawn != NULL)
		i->OnSpawn(i);
	i->IsValid = true;

	CharacterEntityCount++;

	return i;
}

void CharacterOnDespawn(CharacterEntity* c)
{
	if (c->OnDespawn != NULL)
		c->OnDespawn(c);

	if (c->RenderObject != NULL)
	{
		c->RenderObject->IsActive = false;
	}

	c->IsValid = false;
}

void CharacterRotate(CharacterEntity* c, float dir)
{
	c->Rotation = dir;
	c->Direction.x = cosf(c->Rotation);
	c->Direction.y = sinf(c->Rotation);
}

Vector2 CharacterGetForward(CharacterEntity* c, float length)
{
	return Vector2Add(c->Position, Vector2Scale(c->Direction, length));
}

void CharacterTakeDamage(CharacterEntity* c, float amt)
{
	if (c->IsDead)
		return;

	c->Hitpoints -= amt;

	if (c->Hitpoints <= 0)
	{
		c->Hitpoints = 0;
		c->IsDead = true;

		if (c->OnDeath != NULL)
			c->OnDeath(c);
	}
}

void CharacterHeal(CharacterEntity* c, float amt)
{
	if (c->IsDead)
		return;

	c->Hitpoints += amt;
	if (c->Hitpoints > 100)
	{
		c->Hitpoints = 100;
	}
}