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
		.Name = "Knife",
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

	WeaponDataList[1] = (Weapon)
	{
		.Name = "Pistol",
		.WeaponType = WEAPONTYPE_Pistol,
		.MaxAmmo1 = 12,
		.MaxAmmo2 = 30,
		.Damage = 50,
		.IsAutomatic = true,
		.ProjectileSpeed = 5000,
		.FiringTime = 0.1,
		.Spread = 4,
		.ReloadTime = 2,
		.IsMelee = false,
		.OnReload = WeaponOnReload,
		.OnReloadStart = WeaponOnReloadStart,
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
			break;
		}
	}

	Weapon w = { 0 };
	w.WeaponType = refWeapon->WeaponType;

	strcpy_s(w.Name, 32, refWeapon->Name);

	w.CurrentAmmo1 = ammo1;
	w.CurrentAmmo2 = ammo2;

	w.ProjectileSpeed = refWeapon->ProjectileSpeed;
	w.Damage = refWeapon->Damage;

	w.IsMelee = refWeapon->IsMelee;
	w.MaxAmmo1 = refWeapon->MaxAmmo1;
	w.MaxAmmo2 = refWeapon->MaxAmmo2;
	w.FiringTime = refWeapon->FiringTime;
	w.ReloadTime = refWeapon->ReloadTime;
	w.IsAutomatic = refWeapon->IsAutomatic;

	w.Spread = refWeapon->Spread;

	w.OnFire = refWeapon->OnFire;
	w.OnInit = refWeapon->OnInit;
	w.OnReload = refWeapon->OnReload;
	w.OnReloadStart = refWeapon->OnReloadStart;
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

void WeaponOnFire(Weapon* w, Vector2 pos, Vector2 dir)
{
	if (w->_fireTimer > 0)
		return;

	float amt = ((float)GetRandomValue(-100, 100) / 100.0f) * w->Spread;
	amt *= DEG2RAD;

	dir = Vector2Rotate(dir, amt);

	if (!w->IsMelee && w->MaxAmmo1 > 0)
	{
		if (w->CurrentAmmo1 <= 0)
			return;

		w->CurrentAmmo1 -= 1;
		AudioPlay(ToHash("gunshot"), pos);
		ProjectileSpawn(pos, dir, w->ProjectileSpeed, w->Damage);
	}

	w->_fireTimer = w->FiringTime;
	w->_reloadTimer = 0;

	TraceLog(LOG_INFO, "FIRING");
}

void WeaponOnSelect(Weapon* w)
{
	w->_fireTimer = 0;
	w->_reloadTimer = 0;
}

void WeaponOnReloadStart(Weapon* w)
{
	if (w->IsMelee || w->MaxAmmo2 <= 0 || w->_reloadTimer > 0 || w->CurrentAmmo1 >= w->MaxAmmo1)
		return;

	w->_fireTimer = 0;
	w->_reloadTimer = w->ReloadTime;
}

void WeaponOnReload(Weapon* w)
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
}