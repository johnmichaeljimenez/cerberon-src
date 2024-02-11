#include <raylib.h>
#include "character_entity.h"
#include "memory.h"

void CharacterInit()
{
	CharacterEntityListSize = 64;
	CharacterEntityCount = 0;
	CharacterEntityList = MCalloc(CharacterEntityListSize, sizeof(CharacterEntity), "Character List");
}

void CharacterUnload()
{
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

CharacterEntity* CharacterSpawn(Vector2 pos, float rot, float radius, int hp, void* data)
{
	CharacterEntity c = { 0 };
	c.Position = pos;
	c.Rotation = rot;
	c.IsDead = false;
	c.ColliderRadius = radius;

	c.Index = CharacterEntityCount;
	c.IsValid = true;



	CharacterEntityList[CharacterEntityCount] = c;
	CharacterEntityCount++;
	return &CharacterEntityList[CharacterEntityCount];
}

CharacterEntity CharacterOnSpawn(CharacterEntity* c)
{

}

CharacterEntity CharacterOnDespawn(CharacterEntity* c)
{
	if (c->OnDespawn != NULL)
		c->OnDespawn(c);

	if (c->RenderObject != NULL)
	{
		c->RenderObject->IsActive = false;
	}

	c->IsValid = false;
}