#include <raylib.h>
#include <raymath.h>
#include "lighting.h"
#include "camera.h"
#include "utils.h"
#include "asset_manager.h"

Light CreateLight(Vector2 pos, float rot, float sc, float intensity, Color color, bool cs)
{
	Light light = (Light){
		.Position = pos,
		.Rotation = rot,
		.Scale = sc,
		.Intensity = intensity,
		.Color = color,
		.CastShadows = cs
	};

	light._RenderCamera = (Camera2D){
		.zoom = 1,
		.target = pos,
		.offset = (Vector2){ sc / 2, sc / 2 }
	};

	light._RenderTexture = LoadRenderTexture(sc, sc);

	return light;
}

void UpdateLights(int lightCount, Light** lightArray)
{
	for (int i = 0; i < lightCount; i++)
	{
		Light* l = &lightArray[i];
		l->_RenderCamera.target = l->Position;

		BeginTextureMode(l->_RenderTexture);
		ClearBackground(BLACK);
		BeginMode2D(l->_RenderCamera);

		DrawSprite(LightTexture, l->Position, l->Rotation, l->Scale/512, Vector2Zero());//  (l->Position, l->Scale / 2, l->Color);

		EndMode2D();
		EndTextureMode();
	}

}

void DrawLights(int lightCount, Light** lightArray)
{
	BeginBlendMode(BLEND_MULTIPLIED);

	for (int i = 0; i < lightCount; i++)
	{
		BeginBlendMode(BLEND_MULTIPLIED);
		Light* l = &lightArray[i];
		Vector2 pos = l->Position;
		pos.x -= l->Scale / 2;
		pos.y -= l->Scale / 2;

		Rectangle srcRec = { 0, 0, l->_RenderTexture.texture.width, -(float)l->_RenderTexture.texture.height };
		Rectangle destRect = (Rectangle){ l->Position.x, l->Position.y, l->_RenderTexture.texture.width, l->_RenderTexture.texture.height };
		Vector2 origin = { l->_RenderTexture.texture.width / 2, l->_RenderTexture.texture.height / 2 };

		DrawTexturePro(l->_RenderTexture.texture, srcRec, destRect, origin, 0, WHITE);

		BeginBlendMode(BLEND_ADDITIVE);
		DrawTexturePro(l->_RenderTexture.texture, srcRec, destRect, origin, 0, GRAY);

	}
	EndBlendMode();
}