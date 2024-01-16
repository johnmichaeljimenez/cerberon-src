#include <raylib.h>
#include <raymath.h>
#include "i_trigger.h"
#include "mapdata_manager.h"
#include "utils.h"
#include "time.h"
#include "interaction.h"

void TriggerInit()
{
	for (int i = 0; i < CurrentMapData->TriggerCount; i++)
	{
		Trigger* t = &CurrentMapData->Triggers[i];
		t->_timer = 0;
		t->_activated = false;
		t->_currentlyInside = false;

		for (int j = 0; j < t->ColliderCount; j++)
		{
			TriggerCollider* c = &t->Colliders[j];
			c->_rectangle = (Rectangle)
			{
				.x = c->Position.x,
				.y = c->Position.y,
				.width = c->Size.x,
				.height = c->Size.y
			};
		}
	}
}

void TriggerUpdate()
{
	for (int i = 0; i < CurrentMapData->TriggerCount; i++)
	{
		Trigger* t = &CurrentMapData->Triggers[i];

		if (t->OneShot && t->_activated)
		{
			t->_currentlyInside = false;
			continue;
		}

		if (t->Cooldown > 0 && t->_timer > 0)
		{
			t->_timer -= TICKRATE;
			continue;
		}

		Vector2 playerPos = PlayerEntity.Position;

		bool inside = false;
		for (int j = 0; j < t->ColliderCount; j++)
		{
			TriggerCollider* c = &t->Colliders[i];
			inside = CheckCollisionPointRecRotated(playerPos, c->_rectangle, c->Rotation);

			if (inside && !t->_currentlyInside)
			{
				TraceLog(LOG_INFO, "Activated!");
				t->_activated = true;
				if (t->Cooldown > 0)
				{
					t->_currentlyInside = false; //trigger repeats are allowed if there is cooldown
					t->_timer = t->Cooldown;
				}

				Interactable* target = FindInteractable(t->Target);
				if (target != 0)
				{
					target->ChainActivated = true;
					target->DelayTimer = target->Delay;
				}

				goto next; //the correct answer is goto. other opinions are invalid.
			}
		}

	next:
		if (t->Cooldown <= 0)
			t->_currentlyInside = inside;
	}
}