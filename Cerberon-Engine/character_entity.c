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

void CharacterUpdate(CharacterEntity* c)
{

}

CharacterEntity CharacterSpawn(Vector2 pos, float rot, float radius, int hp, void* data)
{

}

CharacterEntity CharacterOnSpawn(CharacterEntity* c)
{

}

CharacterEntity CharacterOnDespawn(CharacterEntity* c)
{

}