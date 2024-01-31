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
	bool IsAutomatic;
	int MaxAmmo1, MaxAmmo2;
	float ProjectileSpeed;
	float Damage;
	float FiringTime;
	float ReloadTime;
	float Spread;
	void(*OnInit)(struct Weapon* w);
	bool(*OnFire)(struct Weapon* w);
	bool(*OnReload)(struct Weapon* w);
	bool(*OnReloadStart)(struct Weapon* w);
	void(*OnSelect)(struct Weapon* w);

	bool _isValid;
	bool _isActive;
	float _reloadTimer;
	float _fireTimer;
} Weapon;

typedef struct WeaponContainer
{
	Weapon Weapons[32];
	Weapon* CurrentWeapon;
	int CurrentWeaponIndex;
} WeaponContainer;

WeaponContainer PlayerWeaponContainer;

void WeaponInitData();
Weapon WeaponGive(WeaponTypes type, int ammo1, int ammo2);
void WeaponUpdate(Weapon* w);
void WeaponOnInit(Weapon* w);
bool WeaponOnFire(Weapon* w, Vector2 pos, Vector2 dir, int height);
void WeaponOnSelect(Weapon* w);
bool WeaponOnReloadStart(Weapon* w);
bool WeaponOnReload(Weapon* w);