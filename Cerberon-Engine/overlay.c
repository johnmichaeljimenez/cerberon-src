#include <raylib.h>
#include "overlay.h"
#include "mapdata_manager.h"
#include "asset_manager.h"
#include "utils.h"

void OverlayInit()
{
	for (int i = 0; i < CurrentMapData->OverlayCount; i++)
	{
		Overlay* o = &CurrentMapData->Overlays[i];

		o->_textureResource = GetTextureResource(ToHash(o->TextureID));
		//TODO: Calculate bounding box from rotated rectangle;
	}
}

void OverlayDraw()
{
	for (int i = 0; i < CurrentMapData->OverlayCount; i++)
	{
		Overlay* o = &CurrentMapData->Overlays[i];
		DrawSprite(o->_textureResource, o->Position, o->Rotation, o->Scale.x, Vector2Zero(), WHITE);
	}
}