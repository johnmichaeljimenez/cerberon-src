#include<raylib.h>
#include "asset_manager.h"
#include <string.h>

void LoadResources()
{

}

void UnloadResources()
{

}

void LoadTexturePack(char* filename, int* arrayCount, TextureResource** array)
{

}

void UnloadTexturePack(int* arrayCount, TextureResource** array)
{

}

TextureResource* GetTextureResource(char* name)
{
	for (int i = 0; i < TextureResourceCount; i++)
	{
		if (strcmp(name, TextureResourceList[i]->Name) == 0)
		{
			return TextureResourceList[i];
		}
	}

	return NULL;
}