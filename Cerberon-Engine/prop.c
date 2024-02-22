#pragma warning(disable:4996)
#include <raylib.h>
#include "prop.h"
#include "memory.h"
#include <stdio.h>

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