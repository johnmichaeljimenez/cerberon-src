using Main.Core;
using Main.Gameplay.Managers;
using Main.Helpers;

namespace Main.Gameplay.Entities;

public class ItemPickupEntity : BaseEntity
{
	public enum ItemTypes
	{
		Health,
		Ammo,
		WeaponAK,
		WeaponShotgun
	}

	[JsonProperty]
	public ItemTypes ItemType { get; set; }

	[JsonProperty]
	public int Amount { get; set; }

	private float lifeTime;
	private const int MAX_LIFETIME = 20;

	public override void Init(GameplayState gameplayState)
	{
		base.Init(gameplayState);

		if (ItemType == ItemTypes.Health)
		{
			Groups.Add("health");
		}
		else if (ItemType == ItemTypes.WeaponAK || ItemType == ItemTypes.WeaponShotgun)
		{
			Groups.Add("weapons");
		}
		else
		{
			Groups.Add("ammo");
		}

		lifeTime = MAX_LIFETIME;
		Log.Send($"Spawned item: {ItemType} at {Position}");
	}

	public override void Update(float dt, float udt)
	{
		base.Update(dt, udt);

		var player = gameplayState.GetManager<PlayerManager>().PlayerCharacter;
		var d = (player.Position - Position).Length();
		if (d <= 1.5f)
		{
			var pickedUp = false;

			if (ItemType == ItemTypes.Health)
				pickedUp = player.Heal(Amount);

			if (ItemType == ItemTypes.Ammo)
				pickedUp = player.Weapons.PickupAmmo();

			if (ItemType == ItemTypes.WeaponAK)
				pickedUp = player.Weapons.PickupWeapon("rifle");

			if (ItemType == ItemTypes.WeaponShotgun)
				pickedUp = player.Weapons.PickupWeapon("shotgun");

			if (pickedUp)
			{
				AudioHandler.PlaySound("generic/item-pickup");
				Despawn();
			}
		}
		else if (d >= 25)
		{
			if (Utils.Countdown(ref lifeTime, dt))
				Despawn();
		}
		else
		{
			lifeTime = MAX_LIFETIME;
		}
	}

	public override void Draw()
	{
		base.Draw();

		Raylib.DrawCircleV(Position, 1, Colors.RED);
	}
}