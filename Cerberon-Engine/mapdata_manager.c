#pragma warning(disable:4996)
#include <raylib.h>
#include <raymath.h>
#include <stdio.h>
#include "mapdata_manager.h"
#include "memory.h"
#include "utils.h"
#include "asset_manager.h"
#include "i_door.h"
#include "i_item.h"
#include <string.h>
#include "camera.h"
#include "i_trigger.h"
#include "renderer.h"

void InitMap()
{
	CurrentMapData = MCalloc(1, sizeof(MapData), "Map Data");
	LoadMap("maps/test.map", CurrentMapData);
}

void UnloadMap()
{
	if (CurrentMapData->TriggerCount > 0)
	{
		for (int i = 0; i < CurrentMapData->TriggerCount; i++)
		{
			MFree(CurrentMapData->Triggers[i].Colliders, CurrentMapData->Triggers[i].ColliderCount, sizeof(TriggerCollider), "Trigger Collider List");
		}

		MFree(CurrentMapData->Triggers, CurrentMapData->TriggerCount, sizeof(Trigger), "Trigger List");
	}

	if (CurrentMapData->LightCount > 0)
	{
		for (int i = 0; i < CurrentMapData->LightCount; i++)
		{
			if (!CurrentMapData->Lights[i].CastShadows)
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

	if (CurrentMapData->TileCount > 0)
		MFree(CurrentMapData->Tiles, CurrentMapData->TileCount, sizeof(Tile), "Tile List");

	MFree(CurrentMapData, 1, sizeof(MapData), "Map Data");
}

void LoadMap(char* filename, MapData* map)
{
	FILE* file = fopen(filename, "rb");

	int n = 0, id = 0;
	float x1, y1, r, x2, y2;
	float _r, _g, _b, s, in;

	//PLAYER
	fread(&x1, sizeof(float), 1, file);
	fread(&y1, sizeof(float), 1, file);
	fread(&r, sizeof(float), 1, file);

	map->PlayerPosition = (Vector2){ x1, y1 };
	map->PlayerRotation = r;

	//WALLS
	fread(&n, sizeof(int), 1, file);
	map->BlockColliderCount = n;
	map->WallCount = n * 4;

	if (map->WallCount > 0)
	{
		//TODO: refactor all freads
		map->BlockColliders = MCalloc(map->BlockColliderCount, sizeof(BlockCollider), "BC List");
		map->Walls = MCalloc(map->WallCount, sizeof(Wall), "Wall List");

		for (int i = 0; i < map->BlockColliderCount; i++)
		{
			BlockCollider bc = { 0 };

			fread(&bc.Position.x, sizeof(float), 1, file);
			fread(&bc.Position.y, sizeof(float), 1, file);
			fread(&bc.Size.x, sizeof(float), 1, file);
			fread(&bc.Size.y, sizeof(float), 1, file);
			fread(&bc.IsCircle, sizeof(bool), 1, file);

			Rectangle blockRect = (Rectangle){ bc.Position.x - (bc.Size.x / 2), bc.Position.y - (bc.Size.y / 2), bc.Size.x, bc.Size.y };
			if (bc.IsCircle)
			{
				blockRect.x = bc.Position.x - bc.Size.x;
				blockRect.y = bc.Position.y - bc.Size.x;
				blockRect.width = bc.Size.x * 2;
				blockRect.height = bc.Size.x * 2;
			}

			map->BlockColliders[i] = bc;
			BlockCollider* _bc = &map->BlockColliders[i];

			CreateRenderObject(RENDERLAYER_Wall, i, blockRect, (void*)_bc, DrawWallBlock, NULL);
		}

		for (int i = 0; i < map->WallCount; i += 4)
		{
			BlockCollider* block = &map->BlockColliders[i / 4];

			if (block->IsCircle)
			{
				map->Walls[i] = (Wall){
					.IsCircle = true,
					.CircleRadius = block->Size.x,
					.CirclePosition = block->Position,
					.WallFlags = WALLFLAG_CAST_SHADOW,
					._Bounds = (Rectangle){
						.x = block->Position.x - (block->Size.x / 2),
						.y = block->Position.y - (block->Size.x / 2),
						.width = block->Size.x,
						.height = block->Size.x,
					}
				};

				continue;
			}

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

	//INTERACTABLES
	fread(&n, sizeof(int), 1, file);
	map->InteractableCount = n;

	if (map->InteractableCount > 0)
	{
		map->Interactables = MCalloc(map->InteractableCount, sizeof(Interactable), "Interactable List");
		for (int i = 0; i < map->InteractableCount; i += 1)
		{
			Interactable in = { 0 };

			fread(&in.InteractableType, sizeof(int), 1, file);
			fread(&in.InteractableSubType, sizeof(int), 1, file);
			fread(&in.Count, sizeof(int), 1, file);
			fread(&in.Flags, sizeof(int), 1, file);
			fread(&in.Position.x, sizeof(float), 1, file);
			fread(&in.Position.y, sizeof(float), 1, file);
			fread(&in.Rotation, sizeof(float), 1, file);
			fread(&in.TargetName, sizeof(char), 32, file);
			fread(&in.Target, sizeof(char), 32, file);
			fread(&in.Delay, sizeof(float), 1, file);
			fread(&in.OneShot, sizeof(bool), 1, file);

			if (in.InteractableType == INTERACTABLE_Door)
			{
				DoorCount++;
			}
			else if (in.InteractableType == INTERACTABLE_Item)
			{
				ItemCount++;
			}

			in.IsActive = true;
			SetInteractableFunctions(&in);
			in.OnInit(&in);
			map->Interactables[i] = in;
		}
	}

	//LIGHTS
	fread(&n, sizeof(int), 1, file);
	n += 1;
	map->LightCount = n;
	map->Lights = MCalloc(map->LightCount, sizeof(Light), "Light List");

	map->Lights[0] = (Light){
		.AlwaysOn = true,
		.CastShadows = true,
		.Color = WHITE,
		.Intensity = 0.7,
		.OnDrawLight = DrawPlayerFlashlight,
		.Position = Vector2Zero(),
		.Rotation = 0,
		.Scale = 1024
	};

	CreateLight(&map->Lights[0]);

	PlayerFlashlight = &map->Lights[0];
	PlayerFlashlight->AlwaysOn = true;

	bool cs;
	for (int i = 1; i < map->LightCount; i += 1)
	{
		Light l = { 0 };

		float _r, _g, _b;

		fread(&l.Position.x, sizeof(float), 1, file);
		fread(&l.Position.y, sizeof(float), 1, file);
		fread(&l.Rotation, sizeof(float), 1, file);
		fread(&l.Scale, sizeof(float), 1, file);
		fread(&l.CastShadows, sizeof(bool), 1, file);
		fread(&_r, sizeof(float), 1, file);
		fread(&_g, sizeof(float), 1, file);
		fread(&_b, sizeof(float), 1, file);
		fread(&l.Intensity, sizeof(float), 1, file);

		_r *= 255;
		_g *= 255;
		_b *= 255;

		l.Color = (Color){ _r, _g, _b, 255 };
		l.OnDrawLight = DrawLightDefault;
		CreateLight(&l);
		map->Lights[i] = l;
	}


	//TILES
	fread(&n, sizeof(int), 1, file);
	map->TileCount = n;

	if (map->TileCount > 0)
	{
		map->Tiles = MCalloc(map->TileCount, sizeof(Tile), "Tile List");
		for (int i = 0; i < map->TileCount; i += 1)
		{
			Tile t = { 0 };

			fread(&t.Position.x, sizeof(float), 1, file);
			fread(&t.Position.y, sizeof(float), 1, file);
			fread(&t.Rotation, sizeof(float), 1, file);
			fread(&t.Scale.x, sizeof(float), 1, file);
			fread(&t.Scale.y, sizeof(float), 1, file);
			fread(&t.TextureID, sizeof(char), 32, file);
			fread(&t.SortIndex, sizeof(int), 1, file);
			fread(&_r, sizeof(float), 1, file);
			fread(&_g, sizeof(float), 1, file);
			fread(&_b, sizeof(float), 1, file);

			t.Tint.r = _r * 255;
			t.Tint.g = _g * 255;
			t.Tint.b = _b * 255;
			t.Tint.a = 255;
			map->Tiles[i] = t;
		}

		TilesInit();
	}


	//TRIGGERS
	fread(&n, sizeof(int), 1, file);
	map->TriggerCount = n;

	if (map->TriggerCount > 0)
	{
		float _r, _g, _b;

		map->Triggers = MCalloc(map->TriggerCount, sizeof(Trigger), "Trigger List");
		for (int i = 0; i < map->TriggerCount; i += 1)
		{
			Trigger t = { 0 };
			fread(&t.Target, sizeof(char), 32, file);
			fread(&t.OneShot, sizeof(bool), 1, file);
			fread(&t.Cooldown, sizeof(float), 1, file);

			fread(&t.HasAmbientLight, sizeof(bool), 1, file);
			fread(&_r, sizeof(float), 1, file);
			fread(&_g, sizeof(float), 1, file);
			fread(&_b, sizeof(float), 1, file);

			t.AmbientLightColor.r = _r * 255;
			t.AmbientLightColor.g = _g * 255;
			t.AmbientLightColor.b = _b * 255;
			t.AmbientLightColor.a = 255;

			fread(&t.ColliderCount, sizeof(float), 1, file);
			if (t.ColliderCount > 0)
			{
				t.Colliders = MCalloc(t.ColliderCount, sizeof(TriggerCollider), "Trigger Collider List");
				for (int j = 0; j < t.ColliderCount; j++)
				{
					t.Colliders[j] = (TriggerCollider){ 0 };

					fread(&t.Colliders[j].Position.x, sizeof(float), 1, file);
					fread(&t.Colliders[j].Position.y, sizeof(float), 1, file);
					fread(&t.Colliders[j].Rotation, sizeof(float), 1, file);
					fread(&t.Colliders[j].Size.x, sizeof(float), 1, file);
					fread(&t.Colliders[j].Size.y, sizeof(float), 1, file);

					t.Colliders[j].Rotation *= -DEG2RAD;
				}
			}

			map->Triggers[i] = t;
		}

		TriggerInit();
	}

	fclose(file);
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
	TriggerUpdate();
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
}

void DrawWallBlock(BlockCollider* b)
{
	if (b->IsCircle)
	{
		DrawCircle(b->Position.x, b->Position.y, b->Size.x, BLACK);
		return;
	}

	Vector2 size = b->Size;
	size.x += 36;
	size.y += 36;

	Vector2 halfSize = Vector2Scale(size, 0.5);
	Rectangle rect = { b->Position.x - halfSize.x, b->Position.y - halfSize.y, size.x, size.y };

	Vector2 origin = { 0, 0 };

	DrawTextureNPatch(WallTexture->Texture, WallNPatch, rect, origin, 0, WHITE);
}

void DrawWalls()
{
	//TODO: Merge this on drawing future overlay function (walls, roof sprites, etc)
	for (int i = 0; i < CurrentMapData->BlockColliderCount; i++)
	{
		BlockCollider* b = &CurrentMapData->BlockColliders[i];
		Vector2 size = b->Size;
		size.x += 36;
		size.y += 36;

		Vector2 halfSize = Vector2Scale(size, 0.5);
		Rectangle rect = { b->Position.x - halfSize.x, b->Position.y - halfSize.y, size.x, size.y };

		Vector2 origin = { 0, 0 };

		if (!CheckCollisionRecs(CameraViewBounds, rect))
			continue;

		DrawTextureNPatch(WallTexture->Texture, WallNPatch, rect, origin, 0, WHITE);
	}
}

void DrawMapHUD(MapData* map)
{

}

void UpdateWall(Wall* w)
{
	Vector2 diff = Vector2Subtract(w->To, w->From);
	w->Normal = GetNormalVector(w->From, w->To);
	w->Length = Vector2Length(diff);
	w->Midpoint = Vector2Add(w->From, w->To);
	w->Midpoint.x /= 2;
	w->Midpoint.y /= 2;

	w->_Bounds = (Rectangle)
	{
		.x = fminf(w->From.x, w->To.x) - 4,
		.y = fminf(w->From.y, w->To.y) - 4,
		.width = fabsf(w->From.x - w->To.x) + 4,
		.height = fabsf(w->From.y - w->To.y) + 4,
	};
}