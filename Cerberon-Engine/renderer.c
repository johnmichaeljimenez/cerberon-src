#include <raylib.h>
#include "renderer.h"
#include "mapdata_manager.h"
#include "camera.h"
#include "memory.h"
#include "tile.h"
#include <stdlib.h>

static int _currentRenderObjectSize;

static void _renderSort(void* a, void* b)
{
	RenderObject* r1 = (RenderObject*)a;
	RenderObject* r2 = (RenderObject*)b;

	if (!r1->IsActive || r1->Data == NULL)
		return 0;

	if (r1->Layer == r2->Layer)
	{
		return (r1->SortingIndex - r2->SortingIndex);
	}

	return r1->Layer - r2->Layer;
}

void RendererInit()
{
	RenderObjectCount = 0;
	_currentRenderObjectSize = 2048;
	RenderObjectList = MCalloc(_currentRenderObjectSize, sizeof(RenderObject), "Render Object List");
	for (int i = 0; i < _currentRenderObjectSize; i++)
	{
		RenderObject* r = &RenderObjectList[i];
		r->IsActive = false;
		r->Data = NULL;
	}

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
	r.IsActive = true;

	RenderObjectList[RenderObjectCount] = r;
	RenderObjectCount++;
	if (RenderObjectCount >= _currentRenderObjectSize)
	{
		_currentRenderObjectSize *= 2;
		RenderObjectList = MRealloc(RenderObjectList, _currentRenderObjectSize, sizeof(RenderObject), _currentRenderObjectSize/2, "Render Object List");
	}

	qsort(RenderObjectList, _currentRenderObjectSize, sizeof(RenderObject), _renderSort);
}

void RendererDraw()
{
	for (int i = 0; i < _currentRenderObjectSize; i++)
	{
		RenderObject* r = &RenderObjectList[i];

		if (!r->IsActive || r->Data == NULL)
			continue;

		if ((r->Bounds.width > 0 && r->Bounds.height > 0) && !CheckCollisionRecs(CameraViewBounds, r->Bounds))
			continue;

		if (r->OnDraw != NULL)
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