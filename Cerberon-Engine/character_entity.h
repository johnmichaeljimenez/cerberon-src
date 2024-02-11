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

	int Hitpoints;
	bool IsDead;

	int Index;
	bool IsValid;

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
CharacterEntity* CharacterSpawn(Vector2 pos, float rot, float radius, int hp, void* data);
CharacterEntity CharacterOnSpawn(CharacterEntity* c);
CharacterEntity CharacterOnDespawn(CharacterEntity* c);
void CharacterRotate(CharacterEntity* p, float dir);
Vector2 CharacterGetForward(CharacterEntity* p, float length);