#include <raylib.h>
#include <raymath.h>
#include "weapon_manager.h"
#include "inventory.h"
#include "player.h"
#include <string.h>

void WeaponInitData()
{
	weaponData[0] = (Weapon)
	{
		.Name = "Knife",
		.CurrentAmmo1 = 0,
		.CurrentAmmo2 = 0,
		.IsMelee = true,
		.OnReload = NULL,
		.OnInit = NULL,
		.OnFire = NULL,
		._isValid = true,
	};

	weaponData[1] = (Weapon)
	{
		.Name = "Pistol",
		.CurrentAmmo1 = 12,
		.CurrentAmmo2 = 30,
		.IsMelee = false,
		.OnReload = NULL,
		.OnInit = NULL,
		.OnFire = NULL,
		._isValid = true,
	};
}

Weapon WeaponGive(Weapon* refWeapon, ItemPickup* refItem, int ammo1, int ammo2)
{
	Weapon w = { 0 };
	
	strcpy_s(w.Name, 32, refWeapon->Name);

	w.itemReference = refItem;
	w.CurrentAmmo1 = refWeapon->CurrentAmmo1;
	w.CurrentAmmo2 = refWeapon->CurrentAmmo2;
	
	w.IsMelee = refWeapon->IsMelee;
	w.MaxAmmo1 = refWeapon->MaxAmmo1;
	w.MaxAmmo2 = refWeapon->MaxAmmo2;
	
	w.OnFire = refWeapon->OnFire;
	w.OnInit = refWeapon->OnInit;
	w.OnReload = refWeapon->OnReload;

	w._timer = 0;
	w._isValid = true;
	w._isActive = true;

	return w;
}

void WeaponUpdate(Weapon* w)
{

}