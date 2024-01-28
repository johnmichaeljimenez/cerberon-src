#include <raylib.h>
#include <raymath.h>
#include "weapon_manager.h"
#include "inventory.h"
#include <string.h>
#include "time.h"

Weapon WeaponDataList[32];
int WeaponDataCount = 32;

void WeaponInitData()
{
	WeaponDataList[0] = (Weapon)
	{
		.Name = "Knife",
		.WeaponType = WEAPONTYPE_Knife,
		.CurrentAmmo1 = 0,
		.CurrentAmmo2 = 0,
		.IsMelee = true,
		.OnReload = NULL,
		.OnInit = WeaponOnInit,
		.OnFire = WeaponOnFire,
		.OnSelect = WeaponOnSelect,
		._isValid = true,
	};

	WeaponDataList[1] = (Weapon)
	{
		.Name = "Pistol",
		.WeaponType = WEAPONTYPE_Pistol,
		.CurrentAmmo1 = 12,
		.CurrentAmmo2 = 30,
		.IsMelee = false,
		.OnReload = WeaponOnReload,
		.OnInit = WeaponOnInit,
		.OnFire = WeaponOnFire,
		.OnSelect = WeaponOnSelect,
		._isValid = true,
	};
}

Weapon WeaponGive(WeaponTypes type, int ammo1, int ammo2)
{
	Weapon* refWeapon = NULL;
	for (int i = 0; i < WeaponDataCount; i++)
	{
		if (WeaponDataList[i].WeaponType == type)
		{
			refWeapon = &WeaponDataList[i];
		}
	}

	Weapon w = { 0 };

	strcpy_s(w.Name, 32, refWeapon->Name);

	w.CurrentAmmo1 = refWeapon->CurrentAmmo1;
	w.CurrentAmmo2 = refWeapon->CurrentAmmo2;

	w.IsMelee = refWeapon->IsMelee;
	w.MaxAmmo1 = refWeapon->MaxAmmo1;
	w.MaxAmmo2 = refWeapon->MaxAmmo2;

	w.OnFire = refWeapon->OnFire;
	w.OnInit = refWeapon->OnInit;
	w.OnReload = refWeapon->OnReload;
	w.OnSelect = refWeapon->OnSelect;

	w._fireTimer = 0;
	w._reloadTimer = 0;
	w._isValid = true;
	w._isActive = true;

	return w;
}

void WeaponUpdate(Weapon* w)
{
	if (w->_fireTimer > 0)
	{
		w->_fireTimer -= TICKRATE;
		if (w->_fireTimer <= 0)
			w->_fireTimer = 0;
	}

	if (w->_reloadTimer > 0)
	{
		w->_reloadTimer -= TICKRATE;
		if (w->_reloadTimer <= 0)
		{
			w->_reloadTimer = 0;
			w->OnReload(w);
		}
	}
}

void WeaponOnInit(Weapon* w)
{
	w->_fireTimer = 0;
	w->_reloadTimer = 0;
}

void WeaponOnFire(Weapon* w)
{
	w->_fireTimer = 0;
	w->_reloadTimer = 0;

	TraceLog(LOG_INFO, "FIRING");
}

void WeaponOnSelect(Weapon* w)
{
	w->_fireTimer = 0;
	w->_reloadTimer = 0;
}

void WeaponOnReload(Weapon* w)
{
	w->_fireTimer = 0;
	w->_reloadTimer = 0;
}