#pragma once
#include <raylib.h>

typedef struct TriggerCollider
{
	Vector2 Position;
	float Rotation;
	Vector2 Size;
} TriggerCollider;

typedef struct Trigger
{
	char* Target[32];
	bool OneShot;
	float Cooldown;

	int ColliderCount;
	TriggerCollider* Colliders;

	bool _activated;
	float _timer;
} Trigger;