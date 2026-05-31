using System.ComponentModel;
using Main.Core;

namespace Main.Gameplay.Entities;

public class BreakableEntity : BaseEntity, ICombatEntity
{
	[JsonProperty]
	[DefaultValue(1)]
	public int HitCount { get; set; }

	[JsonProperty]
	[DefaultValue(2)]
	public float HurtboxRadius { get; set; }

	[JsonProperty]
	public string BreakSpriteName { get; set; }

	public bool IsDead { get; private set; }

	private int hits;

	public override void Init(GameplayState gameplayState)
	{
		base.Init(gameplayState);
		Groups.Add(nameof(ICombatEntity));

		hits = HitCount;
	}

	public bool ApplyDamage(int amt, CharacterEntity from)
	{
		if (hits <= 0)
			return false;

		hits -= amt;

		var dead = hits <= 0;
		OnHit(amt, dead, from);

		if (dead)
		{
			IsDead = true;
			OnDeath();
		}

		return true;
	}

	public void OnDeath()
	{
		Game.Instance.Camera.Shake(3f, null);
		AudioHandler.PlaySound("break", Position);
	}

	public void OnHit(float amt, bool isDead, CharacterEntity from)
	{
		Game.Instance.Camera.Shake(0.8f, null);
		AudioHandler.PlaySound("break", Position);
	}
}