#include <raylib.h>
#include "renderer.h"
#include "mapdata_manager.h"
#include "camera.h"
#include "memory.h"
#include "tile.h"
#include <stdlib.h>

static int _currentRenderObjectSize;

int _renderSort(void* a, void* b)
{
	RenderObject* r1 = (RenderObject*)a;
	RenderObject* r2 = (RenderObject*)b;

	//if (!r1->IsActive || r1->Data == NULL)
		//return -1;

	if (r1->Layer == r2->Layer)
	{
		return (r1->SortingIndex - r2->SortingIndex);
	}

	return r1->Layer - r2->Layer;
}

void RendererInit()
{
	RenderObjectCount = 0;
	_currentRenderObjectSize = 2;
	RenderObjectList = MCalloc(_currentRenderObjectSize, sizeof(RenderObject), "Render Object List");
	for (int i = 0; i < _currentRenderObjectSize; i++)
	{
		RenderObject* r = &RenderObjectList[i];
		r->IsActive = false;
		r->Data = NULL;
		r->SortingIndex = 0;
		r->Layer = 0;
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

	TraceLog(LOG_INFO, "RENDEROBJECT #%d: %d %d %f,%f,%f,%f", RenderObjectCount, r.Layer, r.SortingIndex, r.Bounds.x, r.Bounds.y, r.Bounds.width, r.Bounds.height);

	RenderObjectList[RenderObjectCount] = r;
	RenderObjectCount++;
	if (RenderObjectCount >= _currentRenderObjectSize)
	{
		//TODO: FIX THIS
		int cSize = _currentRenderObjectSize;

		_currentRenderObjectSize *= 4;
		TraceLog(LOG_INFO, "REALLOC RO %d * %d = %d", _currentRenderObjectSize, sizeof(RenderObject), _currentRenderObjectSize * sizeof(RenderObject));
		RenderObject* newList = realloc(RenderObjectList, _currentRenderObjectSize * sizeof(RenderObject));
		if (newList == NULL)
			TraceLog(LOG_ERROR, "RENDER OBJECT LIST REALLOC ERROR");
		else
			RenderObjectList = newList;

		for (int i = cSize; i < _currentRenderObjectSize; i++)
		{
			//TraceLog(LOG_INFO, "REALLOC RO INIT %d", i);
			RenderObjectList[i] = (RenderObject){
				.Data = NULL,
				.IsActive = false,
				.OnDraw = NULL,
				.Layer = 0,
				.SortingIndex = 0
			};
		}
	}

}

void RendererPostInitialize()
{
	qsort(RenderObjectList, _currentRenderObjectSize, sizeof(RenderObject), _renderSort);
}

void RendererDraw()
{
	if (IsKeyPressed(KEY_F3))
	{
		int v = 0;
		for (int i = 0; i < _currentRenderObjectSize; i++)
		{
			RenderObject* r = &RenderObjectList[i];

			if (r->Data != NULL)
				v++;

			if (!r->IsActive || r->Data == NULL)
				continue;

			if (r->Layer == RENDERLAYER_Ground)
			{
				Tile* t = (Tile*)r->Data;
				TraceLog(LOG_INFO, "RENDER OBJECT #%d: %d %d %s", i,  r->Layer, r->SortingIndex, t->TextureID);
			}
			else
			{
				TraceLog(LOG_INFO, "RENDER OBJECT #%d: %d %d", i, r->Layer, r->SortingIndex);
			}
		}

		TraceLog(LOG_INFO, "VV: %d", v);
	}

	for (int i = 0; i < _currentRenderObjectSize; i++)
	{
		RenderObject* r = &RenderObjectList[i];

		if (!r->IsActive || r->Data == NULL)
			continue;

		//if ((r->Bounds.width > 0 && r->Bounds.height > 0) && !CheckCollisionRecs(CameraViewBounds, r->Bounds))
			//continue;

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