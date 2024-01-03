#include <raylib.h>
#include <raymath.h>
#include "collision.h"
#include "mapdata_manager.h"


void MoveBody(Vector2* pos, float radius)
{
	Vector2 p = *pos;

	for (int i = 0; i < CurrentMapData->WallCount; i++)
	{

	}

	*pos = p;
}