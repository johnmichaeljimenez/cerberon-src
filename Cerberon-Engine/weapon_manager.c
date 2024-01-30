#include <raylib.h>
#include <raymath.h>
#include "weapon_manager.h"
#include "inventory.h"
#include <string.h>
#include "time.h"
#include "projectile.h"
#include "audio_manager.h"
#include "utils.h"

Weapon WeaponDataList[32];
int WeaponDataCount = 32;

void WeaponInitData()
{
	WeaponDataList[0] = (Weapon)
	{
		.WeaponType = WEAPONTYPE_Knife,
		.FiringTime = 1.5,
		.ReloadTime = 0,
		.MaxAmmo1 = 0,
		.MaxAmmo2 = 0,
		.IsAutomatic = false,
		.Damage = 30,
		.Spread = 0,
		.IsMelee = true,
		.OnInit = WeaponOnInit,
		.OnFire = WeaponOnFire,
		.OnSelect = WeaponOnSelect,
		.OnReloadStart = NULL,
		.OnReload = NULL,
		._isValid = true,
	};
	strcpy_s(WeaponDataList[0].Name, 32, "Knife");

	WeaponDataList[1] = (Weapon)
	{
		.WeaponType = WEAPONTYPE_Pistol,
		.MaxAmmo1 = 12,
		.MaxAmmo2 = 30,
		.Damage = 50,
		.IsAutomatic = false,
		.ProjectileSpeed = 5000,
		.FiringTime = 0.125,
		.Spread = 4,
		.ReloadTime = 0.625,
		.IsMelee = false,
		.OnReload = WeaponOnReload,
		.OnReloadStart = WeaponOnReloadStart,
		.OnInit = WeaponOnInit,
		.OnFire = WeaponOnFire,
		.OnSelect = WeaponOnSelect,
		._isValid = true,
	};
	strcpy_s(WeaponDataList[1].Name, 32, "Pistol");
}

Weapon WeaponGive(WeaponTypes type, int ammo1, int ammo2)
{
	Weapon* refWeapon = NULL;
	for (int i = 0; i < WeaponDataCount; i++)
	{
		if (WeaponDataList[i].WeaponType == type)
		{
			refWeapon = &WeaponDataList[i];
			break;
		}
	}

	Weapon w = *refWeapon;
	w.WeaponType = refWeapon->WeaponType;

	strcpy_s(w.Name, 32, refWeapon->Name);

	w.CurrentAmmo1 = ammo1;
	w.CurrentAmmo2 = ammo2;

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

bool WeaponOnFire(Weapon* w, Vector2 pos, Vector2 dir)
{
	if (w->_fireTimer > 0)
		return false;

	float amt = GetRandomValueFloat(-1, 1) * w->Spread;
	amt *= DEG2RAD;

	dir = Vector2Rotate(dir, amt);

	if (!w->IsMelee && w->MaxAmmo1 > 0)
	{
		if (w->CurrentAmmo1 <= 0)
			return false;

		w->CurrentAmmo1 -= 1;
		AudioPlay(ToHash("gunshot"), Vector2Add(pos, dir));
		ProjectileSpawn(pos, dir, w->ProjectileSpeed, w->Damage);
	}

	w->_fireTimer = w->FiringTime;
	w->_reloadTimer = 0;

	//TraceLog(LOG_INFO, "FIRING");
	return true;
}

void WeaponOnSelect(Weapon* w)
{
	w->_fireTimer = 0;
	w->_reloadTimer = 0;
}

bool WeaponOnReloadStart(Weapon* w)
{
	if (w->IsMelee || w->MaxAmmo2 <= 0 || w->_reloadTimer > 0 || w->CurrentAmmo1 >= w->MaxAmmo1)
		return false;

	w->_fireTimer = 0;
	w->_reloadTimer = w->ReloadTime;

	return true;
}

bool WeaponOnReload(Weapon* w)
{
	w->_fireTimer = 0;
	w->_reloadTimer = 0;

	w->CurrentAmmo1 += w->CurrentAmmo2;
	w->CurrentAmmo2 = 0;
	if (w->CurrentAmmo1 > w->MaxAmmo1)
	{
		int diff = w->CurrentAmmo1 - w->MaxAmmo1;
		w->CurrentAmmo1 -= diff;
		w->CurrentAmmo2 += diff;
	}

	return true;
}