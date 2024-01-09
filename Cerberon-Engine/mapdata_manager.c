#pragma warning(disable:4996)
#include <raylib.h>
#include <raymath.h>
#include <stdio.h>
#include "mapdata_manager.h"
#include "memory.h"
#include "utils.h"
#include "asset_manager.h"
#include "i_door.h"
#include <string.h>

void InitMap()
{
	CurrentMapData = MCalloc(1, sizeof(MapData), "Map Data");
	LoadMap("maps/test.map", CurrentMapData);
}

void UnloadMap()
{
	if (CurrentMapData->LightCount > 0)
	{
		for (int i = 0; i < CurrentMapData->LightCount; i++)
		{
			UnloadRenderTexture(CurrentMapData->Lights[i]._RenderTexture);
		}

		MFree(CurrentMapData->Lights, CurrentMapData->LightCount, sizeof(Light), "Light List");
	}

	if (CurrentMapData->BlockColliderCount > 0)
		MFree(CurrentMapData->BlockColliders, CurrentMapData->BlockColliderCount, sizeof(BlockCollider), "BC List");

	if (CurrentMapData->WallCount > 0)
		MFree(CurrentMapData->Walls, CurrentMapData->WallCount, sizeof(Wall), "Wall List");

	if (CurrentMapData->InteractableCount > 0)
	{
		for (int i = 0; i < CurrentMapData->InteractableCount; i++)
		{
			CurrentMapData->Interactables[i].OnUnload(&CurrentMapData->Interactables[i]);
		}

		MFree(CurrentMapData->Interactables, CurrentMapData->InteractableCount, sizeof(Interactable), "Interactable List");
	}

	MFree(CurrentMapData, 1, sizeof(MapData), "Map Data");
}

void LoadMap(char* filename, MapData* map)
{
	/*
	 * FORMAT:
	 *
	 * map name [32]
	 *
	 * player pos x
	 * player pos y
	 * rotation
	 *
	 * wall count
	 * - wall 1
	 * -- center x
	 * -- center y
	 * -- width
	 * -- height
	 *
	 * - wall 2
	 * -- ...
	 *
	 * door count
	 * - door 1
	 * -- pos x
	 * -- pos y
	 * -- rotation
	 * -- key id
	 *
	 * - door 2
	 * -- ...
	 *
	 */

	FILE* file = fopen(filename, "rb");

	int n = 0, id = 0;
	float x1, y1, r, x2, y2;

	fread(&x1, sizeof(float), 1, file);
	fread(&y1, sizeof(float), 1, file);
	fread(&r, sizeof(float), 1, file);

	map->PlayerPosition = (Vector2){ x1, y1 };
	map->PlayerRotation = r;

	fread(&n, sizeof(int), 1, file);
	map->BlockColliderCount = n;
	map->WallCount = n * 4;

	if (map->WallCount > 0)
	{
		map->BlockColliders = MCalloc(map->BlockColliderCount, sizeof(BlockCollider), "BC List");
		map->Walls = MCalloc(map->WallCount, sizeof(Wall), "Wall List");

		for (int i = 0; i < map->BlockColliderCount; i++)
		{
			fread(&x1, sizeof(float), 1, file);
			fread(&y1, sizeof(float), 1, file);
			fread(&x2, sizeof(float), 1, file);
			fread(&y2, sizeof(float), 1, file);

			//x1 -= x2 / 2;
			//y1 -= y2 / 2;

			map->BlockColliders[i] = CreateBlockCollider((Vector2) { x1, y1 }, (Vector2) { x2, y2 });
		}

		for (int i = 0; i < map->WallCount; i += 4)
		{
			BlockCollider* block = &map->BlockColliders[i / 4];
			x1 = block->Position.x;
			y1 = block->Position.y;
			x2 = block->Size.x;
			y2 = block->Size.y;

			x2 /= 2;
			y2 /= 2;

			Vector2 a = (Vector2){ x1 - x2, y1 - y2 }; //upper left
			Vector2 b = (Vector2){ x1 - x2, y1 + y2 }; //lower left
			Vector2 c = (Vector2){ x1 + x2, y1 + y2 }; //lower right
			Vector2 d = (Vector2){ x1 + x2, y1 - y2 }; //upper right

			WallFlag flags = WALLFLAG_CAST_SHADOW;

			map->Walls[i] = CreateWall(a, b, flags);
			map->Walls[i + 1] = CreateWall(b, c, flags);
			map->Walls[i + 2] = CreateWall(c, d, flags);
			map->Walls[i + 3] = CreateWall(d, a, flags);
		}
	}


	fread(&n, sizeof(int), 1, file);
	map->InteractableCount = n;

	if (map->InteractableCount > 0)
	{
		int intType;
		int intSubType;
		int flags;
		char* target[32];
		char* targetName[32];

		map->Interactables = MCalloc(map->InteractableCount, sizeof(Interactable), "Interactable List");
		for (int i = 0; i < map->InteractableCount; i += 1)
		{
			fread(&intType, sizeof(int), 1, file);
			fread(&intSubType, sizeof(int), 1, file);
			fread(&flags, sizeof(int), 1, file);
			fread(&x1, sizeof(float), 1, file);
			fread(&y1, sizeof(float), 1, file);
			fread(&r, sizeof(float), 1, file);
			fread(&target, sizeof(char), 32, file);
			fread(&targetName, sizeof(char), 32, file);

			if (intType == INTERACTABLE_Door)
				DoorCount++;

			map->Interactables[i] = CreateInteractable((Vector2) { x1, y1 }, r, target, targetName, intType, intSubType, flags);
		}
	}


	fread(&n, sizeof(int), 1, file);
	n += 1;
	map->LightCount = n;
	map->Lights = MCalloc(map->LightCount, sizeof(Light), "Light List");

	map->Lights[0] = CreateLight(Vector2Zero(), 0, 1024, 0.6, WHITE, true, DrawPlayerFlashlight);
	PlayerFlashlight = &map->Lights[0];

	float _r, _g, _b, s, cs, in;

	for (int i = 1; i < map->LightCount; i += 1)
	{
		fread(&x1, sizeof(float), 1, file);
		fread(&y1, sizeof(float), 1, file);
		fread(&r, sizeof(float), 1, file);
		fread(&s, sizeof(float), 1, file);
		fread(&cs, sizeof(bool), 1, file);
		fread(&_r, sizeof(float), 1, file);
		fread(&_g, sizeof(float), 1, file);
		fread(&_b, sizeof(float), 1, file);
		fread(&in, sizeof(float), 1, file);

		_r *= 255;
		_g *= 255;
		_b *= 255;

		map->Lights[i] = CreateLight((Vector2) { x1, y1 }, r, s, in, (Color) { _r, _g, _b, 255 }, cs, DrawLightDefault);
	}

	fclose(file);
}

BlockCollider CreateBlockCollider(Vector2 pos, Vector2 size)
{
	BlockCollider bc = { 0 };

	bc.Position = pos;
	bc.Size = size;

	return bc;
}

Wall CreateWall(Vector2 from, Vector2 to, WallFlag flags)
{
	Wall w = { 0 };
	w.From = from;
	w.To = to;
	w.WallFlags = flags;

	UpdateWall(&w);

	return w;
}

void UpdateMap(MapData* map)
{
	CheckInteraction();
	for (int i = 0; i < map->InteractableCount; i++)
	{
		Interactable* a = &map->Interactables[i];
		if (!a->IsActive)
			continue;

		a->OnUpdate(a);
	}

	for (int i = 0; i < map->InteractableCount; i++)
	{
		Interactable* a = &map->Interactables[i];
		if (!a->IsActive)
			continue;

		a->OnLateUpdate(a);
	}

	UpdateLights();
}

void DrawMap(MapData* map)
{
	for (int i = 0; i < map->InteractableCount; i++)
	{
		Interactable* a = &map->Interactables[i];
		if (!a->IsActive)
			continue;

		a->OnDraw(a);
	}

	for (int i = 0; i < map->BlockColliderCount; i++)
	{
		BlockCollider* b = &map->BlockColliders[i];
		Vector2 size = b->Size;
		size.x += 36;
		size.y += 36;

		Vector2 halfSize = Vector2Scale(size, 0.5);
		Rectangle rect = { b->Position.x - halfSize.x, b->Position.y - halfSize.y, size.x, size.y };

		Vector2 origin = { 0, 0 };

		DrawTextureNPatch(WallTexture->Texture, WallNPatch, rect, origin, 0, WHITE);
	}

	/*for (int i = 0; i < map->WallCount; i++)
	{
		Wall w = map->Walls[i];

		DrawLineV(w.From, w.To, WHITE);
	}*/

}

void DrawMapHUD(MapData* map)
{
	DrawLights();
}

void UpdateWall(Wall* w)
{
	Vector2 diff = Vector2Subtract(w->To, w->From);
	w->Normal = GetNormalVector(w->From, w->To);
	w->Length = Vector2Length(diff);
	w->Midpoint = Vector2Add(w->From, w->To);
	w->Midpoint.x /= 2;
	w->Midpoint.y /= 2;
}

Interactable CreateInteractable(Vector2 pos, float rot, char* target, char* targetname, InteractableType intType, InteractableSubType intSubType, int flags)
{
	Interactable i = { 0 };

	i.InteractableType = intType;
	i.InteractableSubType = intSubType;
	i.Activated = false;
	i.Flags = flags;
	i.IsActive = true;
	i.Position = pos;
	i.Rotation = rot;
	strcpy_s(i.Target, 32, target);
	strcpy_s(i.TargetName, 32, targetname);

	SetInteractableFunctions(&i);

	i.OnInit(&i);

	return i;
}