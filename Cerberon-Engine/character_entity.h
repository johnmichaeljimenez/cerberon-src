#pragma once
#include <raylib.h>
#include "renderer.h"

typedef struct CharacterEntity
{
	Vector2 Position;
	float Rotation;
	Vector2 Direction;

	RenderObject* RenderObject;

	float ColliderRadius;
	bool HasCollider;
	float MovementSpeed;

	int Hitpoints;
	bool IsDead;

	int Index;
	bool IsValid;

	int Faction;

	void* Data;
	void(*OnSpawn)(struct CharacterEntity* c);
	void(*OnDespawn)(struct CharacterEntity* c);
	void(*OnUpdate)(struct CharacterEntity* c);
	void(*OnLateUpdate)(struct CharacterEntity* c);
	void(*OnDeath)(struct CharacterEntity* c);
} CharacterEntity;

int CharacterEntityListSize;
int CharacterEntityCount;
CharacterEntity* CharacterEntityList;

void CharacterInit();
void CharacterUnload();
void CharacterUpdate();
CharacterEntity* CharacterSpawn(Vector2 pos, float rot, float radius, int hp, void* data, void(*onSpawn)(CharacterEntity* c), void(*onUpdate)(CharacterEntity* c), void(*onLateUpdate)(CharacterEntity* c), void(*onDeath)(CharacterEntity* c));
void CharacterOnDespawn(CharacterEntity* c);
void CharacterRotate(CharacterEntity* c, float dir);
Vector2 CharacterGetForward(CharacterEntity* c, float length);

void CharacterTakeDamage(CharacterEntity* c, float amt);
void CharacterHeal(CharacterEntity* c, float amt);