#include <raylib.h>
#include "prop.h"
#include "memory.h"

Prop* PropCreate(char* ID, Vector2 pos, float rot)
{

}

void PropInit()
{
	PropListMaxSize = 1024;
	PropListCount = 0;
	PropList = MCalloc(PropListMaxSize, sizeof(Prop), "Prop List");
}

void PropUnload()
{
	MFree(PropList, PropListMaxSize, sizeof(PropList), "Prop List");
}

void PropUpdate()
{

}