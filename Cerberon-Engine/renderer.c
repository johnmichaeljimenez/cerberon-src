#include <raylib.h>
#include "renderer.h"

void RendererInit()
{
	RendererScreenTexture = LoadRenderTexture(GetScreenWidth(), GetScreenHeight());
}

void RendererUnload()
{
	UnloadRenderTexture(RendererScreenTexture);
}