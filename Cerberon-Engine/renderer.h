#pragma once
#include <raylib.h>

RenderTexture2D RendererScreenTexture;
RenderTexture2D RendererEffectsTexture;

void RendererInit();
void RendererUnload();
void RendererDraw();