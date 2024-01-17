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

static unsigned long hash;
static LinecastHit lineHit;

static bool isFlashlightOn;

void PlayerInit(PlayerCharacter* p)
{
	hash = ToHash("survivor-idle_shotgun_0");

	p->Position = CurrentMapData->PlayerPosition;
	p->Rotation = CurrentMapData->PlayerRotation;
	p->Direction = (Vector2){ 1, 0 };
	p->CollisionRadius = 32;
	p->InteractionRadius = 180;
	p->MovementSpeed = 200;
	p->CameraOffset = 300;

	InventoryInit(&InventoryPlayer);
}

void PlayerUnload(PlayerCharacter* p)
{
	InventoryUnload(&InventoryPlayer);
}

Vector2 PlayerGetForward(PlayerCharacter* p, float length)
{
	return Vector2Add(p->Position, Vector2Scale(p->Direction, length));
}

void PlayerUpdate(PlayerCharacter* p)
{
	Vector2 vel = Vector2Scale(InputGetMovement(), p->MovementSpeed);
	vel = Vector2Scale(vel, TICKRATE);
	p->Position = Vector2Add(p->Position, vel);

	//collision here
	MoveBody(&p->Position, p->CollisionRadius);

	Vector2 diff = CameraGetMousePosition();
	diff = Vector2Subtract(diff, p->Position);
	float newDir = atan2f(diff.y, diff.x);

	Linecast(p->Position, PlayerGetForward(p, 1300), &lineHit);

	PlayerRotate(p, LerpAngle(p->Rotation, newDir, TICKRATE * 12));
	PlayerFlashlight->Position = PlayerEntity.Position;
	UpdateLightBounds(&PlayerFlashlight);

	if (IsKeyPressed(KEY_F))
	{
		if (InventoryGetItem(&InventoryPlayer, INTERACTABLESUB_ItemFlashlight) != NULL)
		{
			isFlashlightOn = !isFlashlightOn;
		}
	}
}

void PlayerLateUpdate(PlayerCharacter* p)
{
	Vector2 targPos = Vector2Subtract(CameraGetMousePosition(), p->Position);
	targPos = Vector2Add(p->Position, Vector2ClampValue(targPos, 0, 300));

	CameraSetTarget(targPos, false);
	AudioUpdateListenerPosition(p->Position);
}

void PlayerDraw(PlayerCharacter* p)
{
	TextureResource* t = GetTextureResource(hash);

	if (t != NULL)
	{
		DrawSprite(t, p->Position, p->Rotation, 0.6, (Vector2) { -0.15, 0.1 }, WHITE);
	}

	if (lineHit.Hit)
	{
		DrawLineV(lineHit.From, lineHit.To, RED);
		DrawCircleV(lineHit.To, 3, RED);
	}

	DrawCircleLines(p->Position.x, p->Position.y, p->CollisionRadius, GREEN);
	DrawLineV(p->Position, PlayerGetForward(p, p->CollisionRadius), GREEN);
}

void PlayerDrawHUD(PlayerCharacter* p)
{

}


void PlayerRotate(PlayerCharacter* p, float dir)
{
	p->Rotation = dir;
	p->Direction.x = cosf(p->Rotation);
	p->Direction.y = sinf(p->Rotation);
}

void DrawPlayerFlashlight(Light* l)
{
	BeginBlendMode(BLEND_ADDITIVE);

	Color color = ColorBrightness01(l->Color, l->Intensity * 0.5f);
	Color color2 = ColorBrightness01(l->Color, l->Intensity);
	Color color3 = ColorBrightness01(l->Color, l->Intensity * 0.3f);

	if (isFlashlightOn)
	{
		DrawCircleGradient(l->Position.x, l->Position.y, 80, color, BLACK);
		DrawSprite(FlashlightTexture, l->Position, PlayerEntity.Rotation + (90 * DEG2RAD), 2, (Vector2) { 0, 0.5 }, color2);
	}
	else
	{
		DrawCircleGradient(l->Position.x, l->Position.y, 80, color3, BLACK);
	}

	EndBlendMode();

}

void DrawPlayerVision()
{
	BeginBlendMode(BLEND_ADDITIVE);
	Color red = (Color){ 255, 0, 0, 255 };
	Vector2 pos = PlayerEntity.Position;

	DrawCircleGradient(pos.x, pos.y, 64, red, BLACK);
	DrawCircleGradient(pos.x, pos.y, 128, red, BLACK);

	//PLACEHOLDER
	//TODO: replace with actual vision cone sprite
	float length = 1500;
	float angle = 120;
	float halfAngle = angle / 2;
	Vector2 v2, v3;

	v2 = Vector2Rotate((Vector2) { 1, 0 }, PlayerEntity.Rotation + (halfAngle * DEG2RAD));
	v2 = Vector2Add(pos, Vector2Scale(v2, length));

	v3 = Vector2Rotate((Vector2) { 1, 0 }, PlayerEntity.Rotation - (halfAngle * DEG2RAD));
	v3 = Vector2Add(pos, Vector2Scale(v3, length));

	DrawTriangle(pos, v2, v3, red);
	EndBlendMode();
}