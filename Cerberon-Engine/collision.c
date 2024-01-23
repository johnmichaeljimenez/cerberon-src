#include <raylib.h>
#include <raymath.h>
#include "collision.h"
#include "mapdata_manager.h"
#include "i_door.h"
#include <float.h>
#include "utils.h"

bool GetCircleLineIntersection(Vector2 circleCenter, float radius, Vector2 lineStart, Vector2 lineEnd, Vector2* intersectionPoint) {
	Vector2 nearest = WallGetClosestPoint(lineStart, lineEnd, circleCenter);
	float distanceSquared = Vector2DistanceSqr(nearest, circleCenter);
	float c2 = (radius * radius);

	if (distanceSquared > c2)
	{
		return false;
	}

	Vector2 offset = Vector2Scale(Vector2Normalize(Vector2Subtract(lineEnd, lineStart)), sqrtf(c2 - distanceSquared));

	if (Vector2DistanceSqr(circleCenter, lineStart) < c2)
	{
		*intersectionPoint = Vector2Add(nearest, offset);
		return true;
	}
	else
	{
		*intersectionPoint = Vector2Subtract(nearest, offset);
		return true;
	}

	return false;
}

bool GetLineIntersection(float p0_x, float p0_y, float p1_x, float p1_y,
	float p2_x, float p2_y, float p3_x, float p3_y, Vector2* hit)
{
	float s1_x, s1_y, s2_x, s2_y;
	s1_x = p1_x - p0_x; s1_y = p1_y - p0_y;
	s2_x = p3_x - p2_x; s2_y = p3_y - p2_y;

	float s, t;
	s = (-s1_y * (p0_x - p2_x) + s1_x * (p0_y - p2_y)) / (-s2_x * s1_y + s1_x * s2_y);
	t = (s2_x * (p0_y - p2_y) - s2_y * (p0_x - p2_x)) / (-s2_x * s1_y + s1_x * s2_y);

	if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
	{
		hit->x = p0_x + (t * s1_x);
		hit->y = p0_y + (t * s1_y);
		return true;
	}

	hit->x = p1_x;
	hit->y = p1_y;
	return false;
}

Vector2 WallGetClosestPoint(Vector2 p1, Vector2 p2, Vector2 pos)
{
	Vector2 ap = Vector2Subtract(pos, p1);
	Vector2 ab = Vector2Subtract(p2, p1);

	float m = Vector2LengthSqr(ab);
	float d = Vector2DotProduct(ap, ab);

	float dist = d / m;
	if (dist < 0)
		return p1;
	if (dist > 1)
		return p2;

	Vector2 res = (Vector2){ 0,0 };
	res.x = p1.x + ab.x * dist;
	res.y = p1.y + ab.y * dist;
	return res;
}

void MoveBody(Vector2* pos, float radius)
{
	for (int i = 0; i < CurrentMapData->WallCount; i++)
	{
		Wall w = CurrentMapData->Walls[i];

		if (w.IsCircle)
		{
			float distance = Vector2Distance(*pos, w.CirclePosition);
			if (distance > (radius + w.CircleRadius))
				continue;

			float overlap = (radius + w.CircleRadius) - distance;

			// Calculate the unit vector pointing from c1 to c2
			Vector2 direction = Vector2Normalize(Vector2Subtract(w.CirclePosition, *pos));

			// Move circles away from each other
			*pos = Vector2Subtract(*pos, Vector2Scale(direction, overlap / 2));

			continue;
		}

		Vector2 d = Vector2Subtract(*pos, w.Midpoint);

		bool visible = Vector2DotProduct(w.Normal, d) > 0;
		if (!visible)
			continue;

		Vector2 c = WallGetClosestPoint(w.From, w.To, *pos);
		Vector2 cv = Vector2Subtract(c, *pos);
		float cd = Vector2Length(cv);

		if (cd <= radius)
		{
			float rd = cd - radius;
			*pos = Vector2Add(*pos, Vector2Multiply(Vector2Normalize(cv), (Vector2) { rd, rd }));
		}
	}

	for (int i = 0; i < CurrentMapData->InteractableCount; i++)
	{
		Interactable* in = &CurrentMapData->Interactables[i];
		if (in->InteractableType != INTERACTABLE_Door)
			continue;

		Door* door = &DoorList[in->DataIndex];

		if (Vector2DistanceSqr(in->Position, *pos) > door->Length * door->Length)
			continue;

		Vector2 c = WallGetClosestPoint(door->From, door->To, *pos);
		Vector2 cv = Vector2Subtract(c, *pos);
		float cd = Vector2Length(cv);

		float rad = radius + (door->Width / 3);

		if (cd <= rad)
		{
			float rd = cd - rad;
			*pos = Vector2Add(*pos, Vector2Multiply(Vector2Normalize(cv), (Vector2) { rd, rd }));
		}
	}
}

bool Linecast(Vector2 from, Vector2 to, LinecastHit* result)
{
	bool hit = false;
	float lastHit = FLT_MAX;

	result->From = from;
	result->To = to;
	result->WallHit = NULL;
	result->Hit = false;

	for (int i = 0; i < CurrentMapData->InteractableCount; i++)
	{
		Interactable* in = &CurrentMapData->Interactables[i];
		if (in->InteractableType != INTERACTABLE_Door)
			continue;

		Door* door = &DoorList[in->DataIndex];

		Vector2 hitPos;
		bool hasHit = GetLineIntersection(from.x, from.y, to.x, to.y, door->From.x, door->From.y, door->To.x, door->To.y, &hitPos);
		//CheckCollisionLines(from, to, w->From, w->To, &hitPos);

		if (hasHit)
		{
			float diff = Vector2Distance(hitPos, from);

			if (diff < lastHit)
			{
				hit = true;
				result->To = hitPos;
				result->Length = diff;
				result->WallHit = NULL;
				result->Hit = true;
				lastHit = diff;
			}
		}
	}

	for (int i = 0; i < CurrentMapData->WallCount; i++)
	{
		Vector2 hitPos;
		Wall* w = &CurrentMapData->Walls[i];

		if (w->IsCircle)
		{
			if (GetCircleLineIntersection(w->CirclePosition, w->CircleRadius, from, to, &hitPos))
			{
				float diff = Vector2Distance(hitPos, from);

				if (diff < lastHit)
				{
					hit = true;
					result->To = hitPos;
					result->Length = diff;
					result->WallHit = w;
					result->Hit = true;
					lastHit = diff;
				}
			}

			continue;
		}

		Vector2 d = Vector2Subtract(from, w->Midpoint);

		bool visible = Vector2DotProduct(w->Normal, d) > 0;

		if (!visible)
			continue;

		bool hasHit = GetLineIntersection(from.x, from.y, to.x, to.y, w->From.x, w->From.y, w->To.x, w->To.y, &hitPos);
		//CheckCollisionLines(from, to, w->From, w->To, &hitPos);

		if (hasHit)
		{
			float diff = Vector2Distance(hitPos, from);

			if (diff < lastHit)
			{
				hit = true;
				result->To = hitPos;
				result->Length = diff;
				result->WallHit = w;
				result->Hit = true;
				lastHit = diff;
			}
		}
	}

	return hit;
}