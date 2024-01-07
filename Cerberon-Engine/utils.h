#pragma once
#include <raylib.h>
#include "asset_manager.h"

unsigned long ToHash(unsigned char* str);
void DrawSprite(TextureResource* t, Vector2 pos, float rotation, float scale, Vector2 offset, Color color);
bool HasFlag(int from, int to);
Color ColorBrightness01(Color color, float amt);
Vector2 GetNormalVector(Vector2 from, Vector2 to);