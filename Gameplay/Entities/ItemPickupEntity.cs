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
	public ItemTypes ItemType { get; set; }

	[JsonProperty]
	public int Amount { get; set; }

	public override void Init(GameplayState gameplayState)
	{
		base.Init(gameplayState);

		if (ItemType == ItemTypes.Health)
		{
			Groups.Add("health");
		}
		else
		{
			Groups.Add("ammo");
		}

		Log.Send($"Spawned item: {ItemType} at {Position}");
	}

	public override void Update(float dt, float udt)
	{
		base.Update(dt, udt);

		var player = gameplayState.GetManager<PlayerManager>().PlayerCharacter;
		var d = (player.Position - Position);
		if (d.Length() <= 1.5f)
		{
			var pickedUp = false;

			if (ItemType == ItemTypes.Health)
				pickedUp = player.Heal(Amount);

			//TODO: cleanup
			if (ItemType == ItemTypes.AmmoAK)
			{
				player.Weapons.Weapons.FirstOrDefault(p => p.Name == "AK-47").CurrentMaxAmmo += Amount; 
				pickedUp = true;
			}

			if (ItemType == ItemTypes.AmmoSIG)
			{
				player.Weapons.Weapons.FirstOrDefault(p => p.Name == "Sig Sauer").CurrentMaxAmmo += Amount;
				pickedUp = true;
			}

			if (pickedUp)
			{
				AudioHandler.PlaySound("generic/item-pickup");
				Despawn();
			}
		}
	}

	public override void Draw()
	{
		base.Draw();

		Raylib.DrawCircleV(Position, 1, Colors.RED);
	}
}