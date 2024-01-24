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
bool CheckCollisionPointRecRotated(Vector2 point, Rectangle rec, float angle);
void DrawRenderTextureToScreen(Texture* t, float scale);
void DrawBlobShadow(Vector2 pos, float radius, float intensity);
float ClampRelativeAngle(float angle, float reference, float min_offset, float max_offset);
bool GetCircleTangent(Vector2 from, Vector2 circlePos, float radius, Vector2* tanA, Vector2* tanB);
Color LerpColor(Color a, Color b, float t);