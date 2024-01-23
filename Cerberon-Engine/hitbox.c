#include "hitbox.h"

bool CheckHitboxHit(Hitbox* hitbox, int hurtboxCount, Hurtbox* hurtboxList)
{
	bool hit = false;
	for (int i = 0; i < hurtboxCount; i++)
	{
		Hurtbox* h = &hurtboxList[i];
		
		if (!h->IsEnabled)
			continue;

		if (h->Faction == hitbox->Faction)
			continue;

		if (CheckCollisionRecs(h->Bounds, hitbox->Bounds))
		{
			h->OnHit(h, hitbox);
			hit = true;
		}
	}

	return hit;
}