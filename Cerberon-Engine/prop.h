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

typedef struct PropSpriteComponent
{
	char ID[32];
	unsigned long Hash;
	float Scale;
	int SortingGroup;
	int SortingOrder;
	Color Tint;
} PropSpriteComponent;

typedef struct PropComponent
{
	Vector2 Position;
	float Height;
	float Rotation;

	PropComponentType Type;
	void* Data;
	void(*OnInit)(struct Prop* p, struct PropComponent* c);
} PropComponent;

typedef struct Prop
{
	char ID[16];
	int ComponentCount;
	struct PropComponent Components[16];
} Prop;

int PropListMaxSize;
Prop* PropList;

Prop* PropCreate(char* ID, Vector2 pos, float rot);
void PropInit();
void PropUnload();
void PropUpdate();