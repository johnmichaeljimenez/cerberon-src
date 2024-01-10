#include <raylib.h>
#include "renderer.h"

void RendererInit()
{
	RendererScreenTexture = LoadRenderTexture(GetScreenWidth(), GetScreenHeight());
	RendererEffectsTexture = LoadRenderTexture(GetScreenWidth(), GetScreenHeight());
}

void RendererUnload()
{
	UnloadRenderTexture(RendererEffectsTexture);
	UnloadRenderTexture(RendererScreenTexture);
}