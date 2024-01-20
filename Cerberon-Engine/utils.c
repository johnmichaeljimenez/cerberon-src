#include <raylib.h>
#include <raymath.h>
#include "utils.h"
#include <math.h>

Vector2 Vector2RotateAround(Vector2 position, Vector2 origin, float rotation)
{
	Vector2 t = Vector2Subtract(position, origin);
	Vector2 rotatedVector;
	rotatedVector.x = t.x * cosf(rotation) - t.y * sinf(rotation);
	rotatedVector.y = t.x * sinf(rotation) + t.y * cosf(rotation);

	return Vector2Add(rotatedVector, origin);
}

unsigned long ToHash(unsigned char* str) {
	unsigned long hash = 5381;
	int c;

	while (c = *str++) {
		hash = ((hash << 5) + hash) + c; /* hash * 33 + c */
	}

	return hash;
}

void DrawSprite(TextureResource* t, Vector2 pos, float rotation, float scale, Vector2 offset, Color color)
{
	Rectangle src = (Rectangle){ 0, 0, t->Texture.width, t->Texture.height };
	Rectangle dest = (Rectangle){ pos.x, pos.y, t->Texture.width * scale, t->Texture.height * scale };
	Vector2 origin = (Vector2){ dest.width / 2, dest.height / 2 };

	origin = Vector2Add(origin, (Vector2) { dest.width* offset.x, dest.height* offset.y });

	DrawTexturePro(t->Texture, src, dest, origin, rotation * RAD2DEG, color);
}

bool HasFlag(int from, int to)
{
	return from & to;
}

Color ColorBrightness01(Color color, float amt)
{
	return ColorBrightness(color, Remap(amt, 0, 1, -1, 0));
}

Vector2 GetNormalVector(Vector2 from, Vector2 to)
{
	Vector2 diff = Vector2Subtract(to, from);
	return Vector2Normalize((Vector2) { -diff.y, diff.x });
}

float WrapAngle(float from, float to)
{
	float max_angle = PI * 2;
	float difference = fmodf(to - from, max_angle);
	return fmodf(2 * difference, max_angle) - difference;
}

float LerpAngle(float r1, float r2, float t)
{
	return r1 + WrapAngle(r1, r2) * t;
}

bool CheckCollisionPointRecRotated(Vector2 point, Rectangle rec, float angle)
{
	Vector2 origin = (Vector2){ rec.x, rec.y };
	rec.x -= (rec.width / 2);
	rec.y -= (rec.height / 2);
	point = Vector2RotateAround(point, origin, -angle);

	return CheckCollisionPointRec(point, rec);
}

void DrawRenderTextureToScreen(Texture* t, float scale)
{
	Rectangle srcRec = { 0, 0, t->width, -t->height };
	Rectangle destRect = (Rectangle){ 0, 0, t->width * scale , t->height * scale };
	Vector2 origin = { 0,0 };
	DrawTexturePro(*t, srcRec, destRect, origin, 0, WHITE);
}