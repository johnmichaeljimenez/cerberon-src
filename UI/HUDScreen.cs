using Main.Gameplay;
using Main.Gameplay.Entities.Player;
using Main.Gameplay.Managers;
using Main.Helpers;

namespace Main.UI;

public class HUDScreen : BaseScreen
{
	public override string UIGroup => "HUD";
	
	private GameplayState gameplayState;
	private PlayerEntity playerEntity;
	private PlayerWeapons weapons;

	public HUDScreen(object context) : base(context)
	{
		gameplayState = context as GameplayState;

		playerEntity = gameplayState.GetManager<PlayerManager>().PlayerCharacter;
		playerEntity.OnHPChanged.Subscribe(OnHPUpdate).AddTo(disposables);

		weapons = playerEntity.Weapons;
		weapons.OnWeaponAmmoChanged.Subscribe(OnWeaponUpdate).AddTo(disposables);
		weapons.OnWeaponSelected.Subscribe(OnWeaponUpdate).AddTo(disposables);
	}

	public override void OnEnter()
	{
		base.OnEnter();
		
		OnWeaponUpdate(weapons.CurrentWeapon);
		OnHPUpdate(playerEntity.MaxHP);
	}

	private void OnHPUpdate(int amt)
	{
		//TODO: improve
		elements.FirstOrDefault(p => p.ID == "hp-text").Text = $"HP: {amt}/{playerEntity.MaxHP}";
	}

	private void OnWeaponUpdate(Gun w)
	{
		//TODO: improve
		elements.FirstOrDefault(p => p.ID == "ammo-text").Text = $"{w.Name} ({w.CurrentAmmo}/{w.CurrentMaxAmmo})";
	}
}