#pragma once
#include <raylib.h>

typedef enum RenderLayer
{
	RENDERLAYER_Ground,
	RENDERLAYER_Entity,
	RENDERLAYER_Wall,
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
	void(*OnDrawDebug)(void*);
} RenderObject;

RenderObject* CreateRenderObject(RenderLayer renderLayer, int sortingIndex, Rectangle bounds, void* data, void(*onDraw)(void*), void(*onDrawDebug)(void*));
void RendererInit();
void RendererUnload();
void RendererUpdate();
void RendererDraw();
void RendererPostInitialize();
void SetShaderMaskedMode();