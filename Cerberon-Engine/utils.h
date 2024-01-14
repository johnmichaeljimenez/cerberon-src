#pragma once
#include <raylib.h>
#include "asset_manager.h"

Vector2 Vector2RotateAround(Vector2 position, Vector2 origin, float rotation);
unsigned long ToHash(unsigned char* str);
void DrawSprite(TextureResource* t, Vector2 pos, float rotation, float scale, Vector2 offset, Color color);
bool HasFlag(int from, int to);
Color ColorBrightness01(Color color, float amt);
Vector2 GetNormalVector(Vector2 from, Vector2 to);
float WrapAngle(float from, float to);
float LerpAngle(float r1, float r2, float t);