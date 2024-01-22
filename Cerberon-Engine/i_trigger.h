#pragma once
#include <raylib.h>

typedef struct TriggerCollider
{
	Vector2 Position;
	float Rotation;
	Vector2 Size;

	Rectangle _rectangle;
} TriggerCollider;

typedef struct Trigger
{
	char* Target[32];
	bool OneShot;
	float Cooldown;

	bool HasAmbientLight;
	Color AmbientLightColor;

	int ColliderCount;
	TriggerCollider* Colliders;

	bool _activated;
	float _timer;
	bool _currentlyInside;
} Trigger;

void TriggerInit();
void TriggerUpdate();