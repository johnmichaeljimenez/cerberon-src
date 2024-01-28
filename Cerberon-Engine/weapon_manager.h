#pragma once
#include <raylib.h>
#include <raymath.h>
#include "inventory.h"
#include "player.h"

typedef struct Weapon
{
	ItemPickup* itemReference;
	int CurrentAmmo1, CurrentAmmo2;

	char* Name[32];
	bool IsMelee;
	int MaxAmmo1, MaxAmmo2;
	void(*OnInit)(struct Weapon* w);
	void(*OnFire)(struct Weapon* w);
	void(*OnReload)(struct Weapon* w);

	bool _isValid;
	bool _isActive;
	float _timer;
} Weapon;

typedef struct WeaponContainer
{
	Weapon* weapons[32];
	Weapon* CurrentWeapon;
	int CurrentWeaponIndex;
} WeaponContainer;

Weapon weaponData[32];

void WeaponInitData();
Weapon WeaponGive(Weapon* refWeapon, ItemPickup* refItem, int ammo1, int ammo2);
void WeaponUpdate(Weapon* w);