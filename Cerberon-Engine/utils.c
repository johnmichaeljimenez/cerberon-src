#include <raylib.h>
#include "utils.h"

unsigned long ToHash(unsigned char* str) {
	unsigned long hash = 5381;
	int c;

	while (c = *str++) {
		hash = ((hash << 5) + hash) + c; /* hash * 33 + c */
	}

	return hash;
}

void DrawSprite(TextureResource* t, Vector2 pos, float rotation, float scale)
{
	Rectangle src = (Rectangle){ 0, 0, t->Texture.width, t->Texture.height };
	Rectangle dest = (Rectangle){ pos.x, pos.y, t->Texture.width * scale, t->Texture.height * scale };
	Vector2 origin = (Vector2){ dest.width / 2, dest.height / 2 };

	DrawTexturePro(t->Texture, src, dest, origin, rotation * RAD2DEG, WHITE);
}