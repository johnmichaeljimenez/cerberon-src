#pragma once
#include <raylib.h>
#include <raymath.h>
#include "inventory.h"

typedef enum WeaponTypes
{
	WEAPONTYPE_Knife,
	WEAPONTYPE_Pistol,
} WeaponTypes;

typedef struct Weapon
{
	int CurrentAmmo1, CurrentAmmo2;

	char* Name[32];
	WeaponTypes WeaponType;
	bool IsMelee;
	int MaxAmmo1, MaxAmmo2;
	void(*OnInit)(struct Weapon* w);
	void(*OnFire)(struct Weapon* w);
	void(*OnReload)(struct Weapon* w);
	void(*OnSelect)(struct Weapon* w);

	bool _isValid;
	bool _isActive;
	float _timer;
} Weapon;

typedef struct WeaponContainer
{
	Weapon Weapons[32];
	Weapon* CurrentWeapon;
	int CurrentWeaponIndex;
} WeaponContainer;

void WeaponInitData();
Weapon WeaponGive(WeaponTypes type, int ammo1, int ammo2);
void WeaponUpdate(Weapon* w);
void WeaponOnInit(Weapon* w);
void WeaponOnFire(Weapon* w);
void WeaponOnSelect(Weapon* w);
void WeaponOnReload(Weapon* w);