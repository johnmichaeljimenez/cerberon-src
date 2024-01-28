#include <raylib.h>
#include "overlay.h"
#include "mapdata_manager.h"
#include "asset_manager.h"
#include "utils.h"
#include "camera.h"

void OverlayInit()
{
	for (int i = 0; i < CurrentMapData->OverlayCount; i++)
	{
		Overlay* o = &CurrentMapData->Overlays[i];

		o->_textureResource = GetTextureResource(ToHash(o->TextureID));
		//TODO: Calculate bounding box from rotated rectangle;

		float size = fmaxf(o->_textureResource->Texture.width, o->_textureResource->Texture.height);
		size += 32;

		o->_Bounds = (Rectangle)
		{
			.x = o->Position.x - (size/2),
			.y = o->Position.y - (size / 2),
			.width = size,
			.height = size
		};
	}
}

void OverlayDraw()
{
	for (int i = 0; i < CurrentMapData->OverlayCount; i++)
	{
		Overlay* o = &CurrentMapData->Overlays[i];
		if (!CheckCollisionRecs(o->_Bounds, CameraViewBounds))
			continue;

		Vector2 pos = o->Position;
		if (fabsf(o->Height) > 0)
		{
			pos = CameraGetParallaxPosition(o->Position, o->Height);
		}

		DrawSprite(o->_textureResource, pos, o->Rotation, o->Scale.x, Vector2Zero(), WHITE);
	}
}