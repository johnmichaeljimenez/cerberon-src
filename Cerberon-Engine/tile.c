#include <raylib.h>
#include <rlgl.h>
#include "tile.h"
#include "asset_manager.h"
#include "mapdata_manager.h"
#include "utils.h"

void TilesInit()
{
	for (int i = 0; i < CurrentMapData->TileCount; i++)
	{
		Tile* t = &CurrentMapData->Tiles[i];
		float oX = t->Position.x;
		float oY = t->Position.y;
		float hX = t->Scale.x / 2;
		float hY = t->Scale.y / 2;

		t->_textureResource = GetTextureResource(ToHash(t->TextureID));
		t->_meshPoints[0] = Vector2RotateAround((Vector2) { oX-hX, oY-hY }, t->Position, t->Rotation* DEG2RAD);
		t->_meshPoints[1] = Vector2RotateAround((Vector2) { oX-hX, oY+hY }, t->Position, t->Rotation* DEG2RAD);
		t->_meshPoints[2] = Vector2RotateAround((Vector2) { oX+hX, oY+hY }, t->Position, t->Rotation* DEG2RAD);
		t->_meshPoints[3] = Vector2RotateAround((Vector2) { oX+hX, oY-hY }, t->Position, t->Rotation* DEG2RAD);
	}
}

void TilesDraw()
{
	for (int i = 0; i < CurrentMapData->TileCount; i++)
	{
		Tile* t = &CurrentMapData->Tiles[i];

		//DrawTriangleStrip(t->_meshPoints, 4, WHITE);

		//DrawCircleV(t->Position, 200, WHITE);

		DrawLineV(t->_meshPoints[0], t->_meshPoints[1], WHITE);
		DrawLineV(t->_meshPoints[1], t->_meshPoints[2], WHITE);
		DrawLineV(t->_meshPoints[2], t->_meshPoints[3], WHITE);
		DrawLineV(t->_meshPoints[3], t->_meshPoints[0], WHITE);
	}
}