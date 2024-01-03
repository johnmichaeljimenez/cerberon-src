#pragma once
#include <raylib.h>
#include "asset_manager.h"

unsigned long ToHash(unsigned char* str);
void DrawSprite(TextureResource* t, Vector2 pos, float rotation, float scale);