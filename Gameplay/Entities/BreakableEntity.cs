using System.ComponentModel;
using Cerberon.Core;
using Cerberon.Gameplay.Managers;
using Cerberon.Helpers;

namespace Cerberon.Gameplay.Entities;

public class BreakableEntity : BaseEntity, ICombatEntity
{
	[JsonProperty]
	[DefaultValue(1)]
	public int HitCount { get; set; }

	[JsonProperty]
	[DefaultValue(0.5f)]
	public float HurtboxRadius { get; set; }

	[JsonProperty]
	public string BreakSpriteName { get; set; } //TODO: use this

	public bool IsDead { get; private set; }

	[JsonProperty]
	public CombatEntityMaterialType MaterialType { get; set; }

	private int hits;
	private CircleBody circleBody;

	public override void Init(GameplayState gameplayState)
	{
		base.Init(gameplayState);
		Groups.Add(nameof(ICombatEntity));
		circleBody = gameplayState.GetManager<CollisionManager>().AddBody(Position, HurtboxRadius, CollisionHeight.Low, this);

		hits = HitCount;
	}

	public override void Dispose()
	{
		if (circleBody != null)
			gameplayState.GetManager<CollisionManager>().RemoveBody(circleBody);

		base.Dispose();
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
		circleBody.Enabled = false;

		Despawn();
	}

	public void OnHit(float amt, bool isDead, CharacterEntity from)
	{
		Game.Instance.Camera.Shake(0.8f, null);
		AudioHandler.PlaySound("break", Position);
	}

	public override void Draw()
	{
		base.Draw();

		Raylib.DrawCircleV(Position, HurtboxRadius, Color.DarkBlue);
	}
}