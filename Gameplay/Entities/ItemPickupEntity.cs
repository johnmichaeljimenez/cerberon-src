using Cerberon.Core;
using Cerberon.Gameplay.Entities.Player;
using Cerberon.Gameplay.Managers;
using Cerberon.Helpers;

namespace Cerberon.Gameplay.Entities;

public class ItemPickupEntity : BaseEntity, IInteractable
{
	[DataConfig]
	public static Dictionary<ItemTypes, string> ItemSprites = new()
	{
		{ ItemTypes.Health, "item/health"},
		{ ItemTypes.Ammo, "item/ammo"},
		{ ItemTypes.WeaponAK, "item/weapon-ak"},
		{ ItemTypes.WeaponShotgun, "item/weapon-shotgun"},
	};

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

	public InteractionType InteractionType => InteractionType.Pickup;

	private float lifeTime;
	private const int MAX_LIFETIME = 20;

	public override void Init(GameplayState gameplayState)
	{
		base.Init(gameplayState);
		Amount = Math.Max(1, Amount);

		CurrentSpriteID = ItemSprites[ItemType];
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

	public bool Interact()
	{
		var player = gameplayState.GetManager<GameplayManager>().PlayerCharacter;

		if (ItemType == ItemTypes.Health)
			player.PickupHealthItem();

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