#include <raylib.h>
#include <raymath.h>
#include "asset_manager.h"
#include "player.h"
#include "camera.h"
#include "input_handler.h"
#include "utils.h"
#include "mapdata_manager.h"
#include "collision.h"
#include "time.h"
#include "inventory.h"
#include "audio_manager.h"
#include "animation_player.h"
#include "renderer.h"
#include "fsm.h"
#include "weapon_manager.h"
#include "ui_manager.h"

static LinecastHit lineHit;

static bool isFlashlightOn;
static Vector2 lastPos;
static float footstepInterval;

static AnimationPlayerGroup playerAnimation;
static AnimationPlayer idleAnimation;
static AnimationPlayer moveAnimation;
static AnimationPlayer attackAnimation;
static AnimationPlayer handgunShootAnimation;
static AnimationPlayer handgunReloadAnimation;

static AnimationPlayerGroup playerLegAnimation;
static AnimationPlayer legIdleAnimation;
static AnimationPlayer legMoveAnimation;

static float legAngle;
static float legAngleRange = 60;

static bool canStand = true;

void PlayerInit(CharacterEntity* p)
{
	playerAnimation = (AnimationPlayerGroup){ 0 };
	idleAnimation = AnimationPlayerCreate(&playerAnimation, GetAnimationResource(ToHash("player_idle")), AnimationFlag_Physics | AnimationFlag_CanAttack, NULL, NULL, NULL, 24);
	moveAnimation = AnimationPlayerCreate(&playerAnimation, GetAnimationResource(ToHash("player_move")), AnimationFlag_Physics | AnimationFlag_CanAttack, NULL, NULL, NULL, 24);
	attackAnimation = AnimationPlayerCreate(&playerAnimation, GetAnimationResource(ToHash("player_attack")), AnimationFlag_DisableMovement, NULL, NULL, NULL, 24);
	handgunShootAnimation = AnimationPlayerCreate(&playerAnimation, GetAnimationResource(ToHash("player_handgun_fire")), AnimationFlag_None, NULL, NULL, NULL, 24);
	handgunReloadAnimation = AnimationPlayerCreate(&playerAnimation, GetAnimationResource(ToHash("player_handgun_reload")), AnimationFlag_None, NULL, NULL, NULL, 24);

	playerLegAnimation = (AnimationPlayerGroup){ 0 };
	legIdleAnimation = AnimationPlayerCreate(&playerLegAnimation, GetAnimationResource(ToHash("player_leg_idle")), AnimationFlag_None, NULL, NULL, NULL, 24);
	legMoveAnimation = AnimationPlayerCreate(&playerLegAnimation, GetAnimationResource(ToHash("player_leg_move")), AnimationFlag_None, NULL, NULL, NULL, 24);

	attackAnimation.NextAnimation = &idleAnimation;
	handgunShootAnimation.NextAnimation = &idleAnimation;
	handgunReloadAnimation.NextAnimation = &idleAnimation;

	playerAnimation.Animations[0] = &idleAnimation;
	playerAnimation.Animations[1] = &moveAnimation;
	playerAnimation.Animations[2] = &attackAnimation;

	playerLegAnimation.Animations[0] = &legIdleAnimation;
	playerLegAnimation.Animations[1] = &legMoveAnimation;

	p->Position = CurrentMapData->PlayerPosition;
	p->Rotation = CurrentMapData->PlayerRotation;
	p->Direction = (Vector2){ 1, 0 };
	p->ColliderRadius = 32;
	p->MovementSpeed = 150;
	p->Faction = 0;

	p->IsDead = false;
	p->Hitpoints = 100;

	PlayerData.InteractionRadius = 180;
	PlayerData.CameraOffset = 300;
	PlayerData.IsCrouching = false;

	canStand = true;

	PlayerWeaponContainer = (WeaponContainer){
		.CurrentWeaponIndex = -1,
		.CurrentWeapon = NULL
	};

	InventoryInit(&InventoryPlayer, 8);
	InventoryInit(&InventoryPlayerBack, 16);
	InventoryHasPlayerBackpack = false;

	lastPos = p->Position;
	footstepInterval = (p->ColliderRadius * 1.8f);
	footstepInterval *= footstepInterval;

	AnimationPlayerPlay(&playerAnimation, &idleAnimation);
	AnimationPlayerPlay(&playerLegAnimation, &legIdleAnimation);

	CreateRenderObject(RENDERLAYER_Entity, 999, (Rectangle) { 0, 0, 0, 0 }, (void*)p, PlayerDraw, PlayerDrawDebug);
}

void PlayerUnload(CharacterEntity* p)
{
	InventoryUnload(&InventoryPlayerBack);
	InventoryUnload(&InventoryPlayer);
}

void PlayerUpdate(CharacterEntity* p)
{
	if (p->IsDead)
	{
		return;
	}

	Vector2 diff = p->Position;
	float rot = p->Rotation;
	Vector2 movementInput = (Vector2){ 0,0 };
	AnimationPlayer* currentAnimation = playerAnimation.CurrentAnimation;

	if (!HasFlag(currentAnimation->Flags, AnimationFlag_DisableMovement))
	{
		if (InputGetKeyPressed(KEY_LEFT_CONTROL) && (!PlayerData.IsCrouching || canStand))
			PlayerData.IsCrouching = !PlayerData.IsCrouching;

		movementInput = InputGetMovement();
		Vector2 vel = Vector2Scale(movementInput, p->MovementSpeed * (PlayerData.IsCrouching? 0.3 : 1));
		vel = Vector2Scale(vel, GetFrameTime());
		p->Position = Vector2Add(p->Position, vel);

		if (InputGetMouseDirection(p->Position, &diff))
		{
			float newDir = atan2f(diff.y, diff.x);
			rot = LerpAngle(rot, newDir, TICKRATE * 12);
		}

		if (InputGetKeyPressed(KEY_F))
		{
			if (InventoryGetItem(&InventoryPlayer, INTERACTABLESUB_ItemFlashlight) != NULL)
			{
				isFlashlightOn = !isFlashlightOn;
			}
		}
	}

	//collision here
	MoveBody(&p->Position, p->ColliderRadius, PlayerData.IsCrouching, &canStand);

	if (fabsf(movementInput.x) > 0 || fabsf(movementInput.y) > 0)
	{
		AnimationPlayerPlay(&playerLegAnimation, &legMoveAnimation);
		if (!PlayerData.IsCrouching && Vector2DistanceSqr(lastPos, p->Position) > footstepInterval)
		{
			AudioPlay(ToHash(TextFormat("%d", GetRandomValue(0, 8))), p->Position);
			lastPos = p->Position;
		}
	}
	else
	{
		AnimationPlayerPlay(&playerLegAnimation, &legIdleAnimation);
	}

	if (PlayerWeaponContainer.CurrentWeaponIndex >= 0 && PlayerWeaponContainer.CurrentWeapon != NULL)
	{
		if (HasFlag(currentAnimation->Flags, AnimationFlag_CanAttack))
		{
			if (InputGetKeyPressed(KEY_R))
			{
				if (PlayerWeaponContainer.CurrentWeapon->OnReloadStart != NULL)
				{
					if (PlayerWeaponContainer.CurrentWeapon->OnReloadStart(PlayerWeaponContainer.CurrentWeapon))
					{
						AnimationPlayerPlay(&playerAnimation, &handgunReloadAnimation);
					}
				}
			}

			if ((!PlayerWeaponContainer.CurrentWeapon->IsAutomatic && InputGetMousePressed(MOUSE_BUTTON_LEFT))
				||
				(PlayerWeaponContainer.CurrentWeapon->IsAutomatic && InputGetMouseDown(MOUSE_BUTTON_LEFT))
				)
			{
				if (PlayerWeaponContainer.CurrentWeapon->OnFire != NULL)
				{
					Vector2 dir = Vector2Normalize(Vector2Subtract(CharacterGetForward(p, 1), p->Position));
					if (PlayerWeaponContainer.CurrentWeapon->OnFire(PlayerWeaponContainer.CurrentWeapon, p->Position, dir, PlayerData.IsCrouching? 2 : 1))
					{
						AnimationPlayerPlay(&playerAnimation, &handgunShootAnimation);
					}
				}
			}

			if (InputGetMousePressed(MOUSE_BUTTON_RIGHT))
			{

			}
		}
	}
	else
	{
		if (InputGetMousePressed(MOUSE_BUTTON_RIGHT) && InventoryPlayer.CurrentSelectedIndex >= 0)
		{
			ItemPickup* i = InventoryPlayer.Items[InventoryPlayer.CurrentSelectedIndex];
			if (i != NULL && i->ItemStatusType == ITEMSTATUSTYPE_OnInventory && i->CurrentAmount > 0)
			{
				InventoryUse(&InventoryPlayer, i);
			}
		}
	}

	Linecast(p->Position, CharacterGetForward(p, 1300), &lineHit, PlayerData.IsCrouching? 2 : 1);

	CharacterRotate(p, rot);
	PlayerFlashlight->Position = PlayerEntity->Position;
	UpdateLightBounds(&PlayerFlashlight);

	if (IsKeyPressed(KEY_G))
	{
		CharacterTakeDamage(p, 32);
	}

	if (IsKeyPressed(KEY_H))
	{
		CharacterHeal(p, 26);
	}

	SelectInventoryItem(&InventoryPlayer);

	if (PlayerWeaponContainer.CurrentWeapon != NULL)
		WeaponUpdate(PlayerWeaponContainer.CurrentWeapon);
}

void PlayerLateUpdate(CharacterEntity* p)
{
	Vector2 targPos = Vector2Subtract(CameraGetMousePosition(), p->Position);
	targPos = Vector2Add(p->Position, Vector2ClampValue(targPos, 0, UIIsVisible? 50 : 300));

	CameraSetTarget(targPos, false);
	AudioUpdateListenerPosition(p->Position);
	AnimationPlayerUpdate(playerAnimation.CurrentAnimation);
	AnimationPlayerUpdate(playerLegAnimation.CurrentAnimation);
}

void PlayerDraw(CharacterEntity* p)
{
	TextureResource* t = playerAnimation.CurrentAnimation->Clip->SpriteFrames[playerAnimation.CurrentAnimation->CurrentFrame];
	TextureResource* t2 = playerLegAnimation.CurrentAnimation->Clip->SpriteFrames[playerLegAnimation.CurrentAnimation->CurrentFrame];

	DrawBlobShadow(p->Position, p->ColliderRadius * 1.5, 255);

	float halfLegAngle = (legAngleRange / 2) * DEG2RAD;
	legAngle = ClampRelativeAngle(legAngle, p->Rotation, -halfLegAngle, halfLegAngle);

	DrawSprite(t2, p->Position, legAngle, 0.6, (Vector2) { 0, 0 }, WHITE);
	DrawSprite(t, p->Position, p->Rotation, 0.6, (Vector2) { -0.15, 0.1 }, WHITE);
}

void PlayerDrawDebug(CharacterEntity* p)
{
	if (p->IsDead)
		return;

	if (lineHit.Hit)
	{
		DrawLineV(lineHit.From, lineHit.To, RED);
		DrawCircleV(lineHit.To, 3, RED);
	}

	DrawCircleLines(p->Position.x, p->Position.y, p->ColliderRadius, GREEN);
	DrawLineV(p->Position, CharacterGetForward(p, p->ColliderRadius), GREEN);
}

void DrawPlayerFlashlight(Light* l)
{
	BeginBlendMode(BLEND_ADDITIVE);

	Color color = ColorBrightness01(l->Color, l->Intensity * 0.7f);
	Color color2 = ColorBrightness01(l->Color, l->Intensity);
	Color color3 = ColorBrightness01(l->Color, l->Intensity * 0.5f);

	if (isFlashlightOn)
	{
		DrawCircleGradient(l->Position.x, l->Position.y, 80, color, BLACK);
		DrawSprite(FlashlightTexture, l->Position, PlayerEntity->Rotation + (90 * DEG2RAD), 2, (Vector2) { 0, 0.5 }, color2);
	}
	else
	{
		DrawCircleGradient(l->Position.x, l->Position.y, 80, color3, BLACK);
	}

	EndBlendMode();

}

void DrawPlayerVision()
{
	//BeginBlendMode(BLEND_ADDITIVE);
	Color red = (Color){ 255, 0, 0, 255 };
	Vector2 pos = PlayerEntity->Position;

	DrawCircleGradient(pos.x, pos.y, 100, red, ColorAlpha(red, 0));
	//DrawCircleGradient(pos.x, pos.y, 128, red, BLACK);

	//PLACEHOLDER
	//TODO: replace with actual vision cone sprite
	float length = 1800;
	float angle = 120;
	float halfAngle = angle / 2;
	Vector2 v2, v3;

	v2 = Vector2Rotate((Vector2) { 1, 0 }, PlayerEntity->Rotation + (halfAngle * DEG2RAD));
	v2 = Vector2Add(pos, Vector2Scale(v2, length));

	v3 = Vector2Rotate((Vector2) { 1, 0 }, PlayerEntity->Rotation - (halfAngle * DEG2RAD));
	v3 = Vector2Add(pos, Vector2Scale(v3, length));

	DrawTriangle(pos, v2, v3, red);

	//EndBlendMode();
}

void SelectInventoryItem(InventoryContainer* in)
{
	int n = 0;
	for (int i = KEY_ONE; i < KEY_EIGHT + 1; i++)
	{
		if (InputGetKeyPressed(i))
		{
			in->CurrentSelectedIndex = n;

			if (in->Items[n] == NULL)
			{
				PlayerWeaponContainer.CurrentWeapon = NULL;
				continue;
			}

			InteractableSubType t = in->Items[n]->Interactable->InteractableSubType;

			PlayerWeaponContainer.CurrentWeaponIndex = n;

			if (t != INTERACTABLESUB_ItemWeaponPistol)
			{
				PlayerWeaponContainer.CurrentWeapon = NULL;
				continue;
			}

			PlayerWeaponContainer.CurrentWeapon = &PlayerWeaponContainer.Weapons[n];

			if (PlayerWeaponContainer.CurrentWeapon->OnSelect != NULL)
				PlayerWeaponContainer.CurrentWeapon->OnSelect(PlayerWeaponContainer.CurrentWeapon);
		}

		n++;
	}
}

void PlayerAddWeapon(struct Weapon w, struct ItemPickup* i)
{
	PlayerWeaponContainer.Weapons[i->CurrentSlotIndex] = w;
}