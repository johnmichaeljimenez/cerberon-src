#include <raylib.h>
#include "renderer.h"
#include "mapdata_manager.h"
#include "camera.h"
#include "memory.h"
#include "tile.h"
#include <stdlib.h>
#include "utils.h"

static bool lightingEnabled;
static bool debugEnabled;
static int RenderObjectCount;
static RenderObject* RenderObjectList;

static RenderTexture2D LightRenderTexture;
static RenderTexture2D RendererScreenTexture;
static RenderTexture2D RendererEffectsTexture;
static int _currentRenderObjectSize;

static Shader lightShader;
static int screenTexParam;
static int effectsTexParam;

static float screenLightScale = 2;

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
	lightingEnabled = true;
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
		r->OnDrawDebug = NULL;
		r->OnDraw = NULL;
	}

	RendererScreenTexture = LoadRenderTexture(GetScreenWidth(), GetScreenHeight());
	RendererEffectsTexture = LoadRenderTexture(GetScreenWidth() / 2, GetScreenHeight() / 2);
	LightRenderTexture = LoadRenderTexture(GetScreenWidth() / screenLightScale, GetScreenHeight() / screenLightScale);


	lightShader = LoadShader(0, "res/gfx/lighting.frag");
	screenTexParam = GetShaderLocation(lightShader, "screenTex");
	effectsTexParam = GetShaderLocation(lightShader, "effectTex");
}

void RendererUnload()
{
	UnloadShader(lightShader);
	MFree(RenderObjectList, _currentRenderObjectSize, sizeof(RenderObject), "Render Object List");

	UnloadRenderTexture(LightRenderTexture);
	UnloadRenderTexture(RendererEffectsTexture);
	UnloadRenderTexture(RendererScreenTexture);
}

void RendererUpdate()
{
	if (IsKeyPressed(KEY_F2))
		lightingEnabled = !lightingEnabled;
	if (IsKeyPressed(KEY_F3))
		debugEnabled = !debugEnabled;

	if (IsKeyPressed(KEY_F6))
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
				TraceLog(LOG_INFO, "RENDER OBJECT #%d: %d %d %s", i, r->Layer, r->SortingIndex, t->TextureID);
			}
			else
			{
				TraceLog(LOG_INFO, "RENDER OBJECT #%d: %d %d", i, r->Layer, r->SortingIndex);
			}
		}

		TraceLog(LOG_INFO, "VV: %d", v);
	}
}

RenderObject* CreateRenderObject(RenderLayer renderLayer, int sortingIndex, Rectangle bounds, void* data, void(*onDraw)(void*), void(*onDrawDebug)(void*))
{
	RenderObject r = { 0 };
	r.Layer = renderLayer;
	r.SortingIndex = sortingIndex;
	r.Bounds = bounds;
	r.Data = data;
	r.OnDraw = onDraw;
	r.IsActive = true;
	r.OnDrawDebug = onDrawDebug;

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

	return &r;
}

void RendererPostInitialize()
{
	qsort(RenderObjectList, _currentRenderObjectSize, sizeof(RenderObject), _renderSort);
}

static void _DrawWorld()
{
	BeginTextureMode(RendererScreenTexture);
	ClearBackground(BLACK);

	BeginMode2D(GameCamera);

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

	EndMode2D();
	EndTextureMode();
}

void _DrawDebug()
{
	BeginMode2D(GameCamera);

	for (int i = 0; i < _currentRenderObjectSize; i++)
	{
		RenderObject* r = &RenderObjectList[i];

		if (!r->IsActive || r->Data == NULL)
			continue;

		if ((r->Bounds.width > 0 && r->Bounds.height > 0))
		{
			if (!CheckCollisionRecs(CameraViewBounds, r->Bounds))
				continue;

			DrawRectangleLines(r->Bounds.x, r->Bounds.y, r->Bounds.width, r->Bounds.height, WHITE);
		}

		if (r->OnDrawDebug != NULL)
			r->OnDrawDebug(r->Data);
	}

	EndMode2D();
}

void RendererDraw()
{
	_DrawWorld();

	if (lightingEnabled)
	{
		UpdateLights(&RendererScreenTexture, &RendererEffectsTexture, &LightRenderTexture);

		BeginShaderMode(lightShader);
		SetShaderValueTexture(lightShader, screenTexParam, RendererScreenTexture.texture);
		SetShaderValueTexture(lightShader, effectsTexParam, RendererEffectsTexture.texture);
		DrawRenderTextureToScreen(&LightRenderTexture.texture, screenLightScale);
		EndShaderMode();
	}
	else
	{
		DrawRenderTextureToScreen(&RendererScreenTexture.texture, 1);
	}

	if (debugEnabled)
		_DrawDebug();

	PlayerDrawHUD(&PlayerEntity);
}