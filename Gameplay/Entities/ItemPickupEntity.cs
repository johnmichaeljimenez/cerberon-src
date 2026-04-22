using Main.Core;
using Main.Gameplay.Managers;
using Main.Helpers;

namespace Main.Gameplay.Entities;

public class ItemPickupEntity : BaseEntity
{
	public enum ItemTypes
	{
		Health,
		AmmoSIG,
		AmmoAK
	}

	[JsonProperty]
	public ItemTypes ItemType { get; private set; }

	[JsonProperty]
	public int Amount { get; private set; }

	public override void Update(float dt, float udt)
	{
		base.Update(dt, udt);

		var player = gameplayState.GetManager<PlayerManager>().PlayerCharacter;
		var d = (player.Position - Position);
		if (d.Length() <= 0.5f)
		{
			var pickedUp = false;

			if (ItemType == ItemTypes.Health)
				pickedUp = player.Heal(Amount);

			if (pickedUp)
				Despawn();
		}
	}

	public override void Draw()
	{
		base.Draw();

		Raylib.DrawCircleV(Position, 1, Colors.RED);
	}
}