#include <raylib.h>
#include "renderer.h"

void RendererInit()
{
	RendererScreenTexture = LoadRenderTexture(GetScreenWidth(), GetScreenHeight());
	RendererEffectsTexture = LoadRenderTexture(GetScreenWidth() / 2, GetScreenHeight() / 2);
}

void RendererUnload()
{
	UnloadRenderTexture(RendererEffectsTexture);
	UnloadRenderTexture(RendererScreenTexture);
}