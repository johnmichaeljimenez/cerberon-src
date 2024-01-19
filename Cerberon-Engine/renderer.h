#pragma once
#include <raylib.h>

typedef enum RenderLayer
{
	RENDERLAYER_Ground,
	RENDERLAYER_Entity,
	RENDERLAYER_Wall,
	RENDERLAYER_Light,
	RENDERLAYER_Overlay,
} RenderLayer;

typedef struct RenderObject
{
	bool IsActive;
	RenderLayer Layer;
	int SortingIndex;
	Rectangle Bounds;
	void* Data;
	void(*OnDraw)(void*);
} RenderObject;

int RenderObjectCount;
RenderObject* RenderObjectList;

RenderTexture2D RendererScreenTexture;
RenderTexture2D RendererEffectsTexture;

void CreateRenderObject(RenderLayer renderLayer, int sortingIndex, Rectangle bounds, void* data, void(*onDraw)(void*));
void RendererInit();
void RendererUnload();
void RendererDraw();
void RendererPostInitialize();