#include <raylib.h>
#include <rlgl.h>
#include "tile.h"
#include "asset_manager.h"
#include "mapdata_manager.h"
#include "utils.h"
#include <stdlib.h>

int _sort(const void* a, const void* b)
{
	Tile* tA = (Tile*)a;
	Tile* tB = (Tile*)b;

	return (tA->SortIndex - tB->SortIndex);
}

void TilesInit()
{
	for (int i = 0; i < CurrentMapData->TileCount; i++)
	{
		Tile* t = &CurrentMapData->Tiles[i];
		float oX = t->Position.x;
		float oY = t->Position.y;
		float hX = t->Scale.x / 2;
		float hY = t->Scale.y / 2;

		Vector2 tScale = (Vector2){ t->Scale.x / 64, t->Scale.y / 64 };

		t->_textureResource = GetTextureResource(ToHash(t->TextureID));
		t->_meshPoints[0] = Vector2RotateAround((Vector2) { oX - hX, oY - hY }, t->Position, t->Rotation* DEG2RAD);
		t->_meshPoints[1] = Vector2RotateAround((Vector2) { oX - hX, oY + hY }, t->Position, t->Rotation* DEG2RAD);
		t->_meshPoints[2] = Vector2RotateAround((Vector2) { oX + hX, oY + hY }, t->Position, t->Rotation* DEG2RAD);
		t->_meshPoints[3] = Vector2RotateAround((Vector2) { oX + hX, oY - hY }, t->Position, t->Rotation* DEG2RAD);

		t->_uvPoints[0] = (Vector2){ 0,0 };
		t->_uvPoints[1] = (Vector2){ 0,tScale.y };
		t->_uvPoints[2] = (Vector2){ tScale.x,tScale.y };
		t->_uvPoints[3] = (Vector2){ tScale.x,0 };
	}

	qsort(CurrentMapData->Tiles, CurrentMapData->TileCount, sizeof(Tile), _sort);
}

void TilesDraw()
{
	for (int i = 0; i < CurrentMapData->TileCount; i++)
	{
		Tile* t = &CurrentMapData->Tiles[i];

		rlSetTexture(t->_textureResource->Texture.id);
		rlBegin(RL_QUADS);

		rlColor4ub(t->Tint.r, t->Tint.g, t->Tint.b, t->Tint.a);

		for (int j = 0; j < 4; j++)
		{
			rlTexCoord2f(t->_uvPoints[j].x, t->_uvPoints[j].y);
			rlVertex2f(t->_meshPoints[j].x, t->_meshPoints[j].y);
		}

		rlEnd();
		rlSetTexture(0);

		//DrawLineV(t->_meshPoints[0], t->_meshPoints[1], t->Tint);
		//DrawLineV(t->_meshPoints[1], t->_meshPoints[2], t->Tint);
		//DrawLineV(t->_meshPoints[2], t->_meshPoints[3], t->Tint);
		//DrawLineV(t->_meshPoints[3], t->_meshPoints[0], t->Tint);
	}
}