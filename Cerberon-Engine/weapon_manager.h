#pragma once
#include <raylib.h>
#include <raymath.h>
#include "inventory.h"
#include "player.h"

typedef struct Weapon
{
	ItemPickup* itemData;
	int CurrentAmmo1, CurrentAmmo2;

	char* Name[32];
	int MaxAmmo1, MaxAmmo2;
	void(*OnInit)(struct Weapon* w);
	void(*OnFire)(struct Weapon* w);
	void(*OnReload)(struct Weapon* w);

	bool _isActive;
	float _timer;
} Weapon;

void WeaponInitData();
void WeaponLoad(Weapon* w);
void WeaponUpdate(Weapon* w);