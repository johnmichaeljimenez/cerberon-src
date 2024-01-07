#include <raylib.h>
#include <raymath.h>
#include "utils.h"

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

	origin = Vector2Add(origin, (Vector2) { dest.width * offset.x, dest.height * offset.y });

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