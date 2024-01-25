#pragma once
#include <raylib.h>
#include "asset_manager.h"

typedef struct Overlay
{
	Vector2 Position;
	float Rotation;
	Vector2 Scale;
	char* TextureID[32];
	float Alpha;

	TextureResource* _textureResource;
	Rectangle _Bounds;
} Overlay;

void OverlayInit();
void OverlayDraw();