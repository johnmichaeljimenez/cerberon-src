#pragma warning(disable:4996)
#include <raylib.h>
#include "prop.h"
#include "memory.h"
#include <stdio.h>
#include "utils.h"

Prop* PropCreate(char* ID, Vector2 pos, float rot)
{

}

void PropInit()
{
	FILE* file = fopen("maps/prefab.dat", "rb");

	fread(&PropListMaxSize, sizeof(int), 1, file);
	PropList = MCalloc(PropListMaxSize, sizeof(Prop), "Prop List");

	for (int i = 0; i < PropListMaxSize; i++)
	{
		Prop p = { 0 };
		fread(&p.ID, sizeof(char), 16, file);
		fread(&p.ComponentCount, sizeof(int), 1, file);

		for (int j = 0; j < p.ComponentCount; j++)
		{
			PropComponent c = { 0 };
			fread(&c.Position.x, sizeof(float), 1, file);
			fread(&c.Position.y, sizeof(float), 1, file);
			fread(&c.Height, sizeof(float), 1, file);
			fread(&c.Rotation, sizeof(float), 1, file);
			fread(&c.Type, sizeof(int), 1, file);

			if (c.Type == PROPCOMPONENTTYPE_Sprite)
			{
				PropSpriteComponent d = { 0 };
				float r, g, b;
				fread(&d.ID, sizeof(char), 32, file);
				fread(&d.Scale, sizeof(float), 1, file);
				fread(&d.SortingGroup, sizeof(int), 1, file);
				fread(&d.SortingOrder, sizeof(int), 1, file);
				fread(&r, sizeof(float), 1, file);
				fread(&g, sizeof(float), 1, file);
				fread(&b, sizeof(float), 1, file);

				r *= 255;
				g *= 255;
				b *= 255;

				d.Tint = (Color){ r, g, b, 255 };
				d.Hash = ToHash(d.ID);
				c.Data = &d;
			}
			else if (c.Type == PROPCOMPONENTTYPE_BoxCollider)
			{
				PropBoxColliderComponent d = { 0 };
				fread(&d.Size.x, sizeof(float), 1, file);
				fread(&d.Size.y, sizeof(float), 1, file);

				c.Data = &d;
			}

			p.Components[j] = c;
		}

		PropList[i] = p;
	};

	fclose(file);
}

void PropUnload()
{
	MFree(PropList, PropListMaxSize, sizeof(PropList), "Prop List");
}

void PropUpdate()
{

}