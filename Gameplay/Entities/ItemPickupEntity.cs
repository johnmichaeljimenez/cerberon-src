using Main.Core;
using Main.Gameplay.Entities.Player;
using Main.Gameplay.Managers;
using Main.Helpers;

namespace Main.Gameplay.Entities;

public class ItemPickupEntity : BaseEntity, IInteractable
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

	public bool Interactable => true;

	private float lifeTime;
	private const int MAX_LIFETIME = 20;

	public override void Init(GameplayState gameplayState)
	{
		base.Init(gameplayState);

		Groups.Add(nameof(IInteractable));

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

		var player = gameplayState.GetManager<GameplayManager>().PlayerCharacter;
		var d = (player.Position - Position).Length();
		if (d >= 25 && !SpawnedIngame)
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

	public bool Interact()
	{
		var player = gameplayState.GetManager<GameplayManager>().PlayerCharacter;

		if (ItemType == ItemTypes.Health)
			player.Heal(Amount);

		if (ItemType == ItemTypes.Ammo)
			player.Weapons.PickupAmmo();

		if (ItemType == ItemTypes.WeaponAK)
			player.Weapons.PickupWeapon("rifle");

		if (ItemType == ItemTypes.WeaponShotgun)
			player.Weapons.PickupWeapon("shotgun");

		AudioHandler.PlaySound("generic/item-pickup");
		Despawn();
		
		return true;
	}
}