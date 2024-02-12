#pragma once
#include <raylib.h>

typedef enum PropComponentType
{
	PROPCOMPONENTTYPE_Sprite,
	PROPCOMPONENTTYPE_SpriteTiled,
	PROPCOMPONENTTYPE_BoxCollider,
	PROPCOMPONENTTYPE_CircleCollider,
	PROPCOMPONENTTYPE_EdgeCollider,
	PROPCOMPONENTTYPE_Light
} PropComponentType;

typedef struct PropComponent
{
	PropComponentType Type;
	void* Data;
	void(*OnInit)(struct Prop* p, struct PropComponent* c);
};

typedef struct Prop
{
	Vector2 Position;
	float Rotation;
	char ID[16];
	struct PropComponent Components[16];
} Prop;

int PropListMaxSize;
int PropListCount;
Prop* PropList;

Prop* PropCreate(char* ID, Vector2 pos, float rot);
void PropInit();
void PropUnload();
void PropUpdate();