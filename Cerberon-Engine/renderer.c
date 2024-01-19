#include <raylib.h>
#include "renderer.h"
#include "mapdata_manager.h"
#include "camera.h"
#include "memory.h"
#include "tile.h"

static int _currentRenderObjectSize;

void RendererInit()
{
	RenderObjectCount = 0;
	_currentRenderObjectSize = 2048;
	RenderObjectList = MCalloc(_currentRenderObjectSize, sizeof(RenderObject), "Render Object List");

	RendererScreenTexture = LoadRenderTexture(GetScreenWidth(), GetScreenHeight());
	RendererEffectsTexture = LoadRenderTexture(GetScreenWidth() / 2, GetScreenHeight() / 2);
}

void RendererUnload()
{
	MFree(RenderObjectList, _currentRenderObjectSize, sizeof(RenderObject), "Render Object List");

	UnloadRenderTexture(RendererEffectsTexture);
	UnloadRenderTexture(RendererScreenTexture);
}

void CreateRenderObject(RenderLayer renderLayer, int sortingIndex, Rectangle bounds, void* data, void(*onDraw)(void*))
{
	RenderObject r = { 0 };
	r.Layer = renderLayer;
	r.SortingIndex = sortingIndex;
	r.Bounds = bounds;
	r.Data = data;
	r.OnDraw = onDraw;

	RenderObjectList[RenderObjectCount] = r;
	RenderObjectCount++;
	if (RenderObjectCount >= _currentRenderObjectSize)
	{
		_currentRenderObjectSize *= 2;
		RenderObjectList = MRealloc(RenderObjectList, _currentRenderObjectSize, sizeof(RenderObject), _currentRenderObjectSize/2, "Render Object List");
	}
}

void RendererDraw()
{
	for (int i = 0; i < RenderObjectCount; i++)
	{
		RenderObject* r = &RenderObjectList[i];

		if ((r->Bounds.width > 0 && r->Bounds.height > 0) && !CheckCollisionRecs(CameraViewBounds, r->Bounds))
			continue;

		if (r->Data != NULL && r->OnDraw != NULL)
			r->OnDraw(r->Data);
	}

	//tiles
	//decals
	//low props
	//entities
	//high props
	//lights
	//overlay

	//debug
	//hud
}