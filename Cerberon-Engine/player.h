#pragma once
#include <raylib.h>
#include <raymath.h>
#include "asset_manager.h"
#include "character_entity.h"

typedef struct PlayerCharacterData
{
	float MovementSpeed;
	float InteractionRadius;
	float CameraOffset;

	bool IsCrouching;

} PlayerCharacterData;

PlayerCharacterData PlayerData;
CharacterEntity* PlayerEntity;
struct Light* PlayerFlashlight;

void PlayerInit(CharacterEntity* p);
void PlayerUnload(CharacterEntity* p);
void PlayerUpdate(CharacterEntity* p);
void PlayerLateUpdate(CharacterEntity* p);
void PlayerDraw(CharacterEntity* p);
void PlayerDrawDebug(CharacterEntity* p);

void SelectInventoryItem(struct InventoryContainer* in);
void PlayerAddWeapon(struct Weapon w, struct ItemPickup* i);

void DrawPlayerFlashlight(struct Light* l);
void DrawPlayerVision();