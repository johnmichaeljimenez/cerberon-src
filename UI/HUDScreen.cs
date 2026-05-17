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
		gameplayState.GetManager<GameplayManager>().OnFightStart.Subscribe(_ => UpdateVisibility()).AddTo(disposables);
		gameplayState.GetManager<DialogueManager>().OnDialogueShow.Subscribe(_ => UpdateVisibility()).AddTo(disposables);

		playerEntity = gameplayState.GetManager<GameplayManager>().PlayerCharacter;
		playerEntity.OnHPChanged.Subscribe(OnHPUpdate).AddTo(disposables);

		weapons = playerEntity.Weapons;
		weapons.OnWeaponAmmoChanged.Subscribe(OnWeaponUpdate).AddTo(disposables);
		weapons.OnWeaponSelected.Subscribe(OnWeaponUpdate).AddTo(disposables);
	}

	public override void OnEnter()
	{
		base.OnEnter();

		UpdateVisibility();

		OnWeaponUpdate(weapons.CurrentWeapon);
		OnHPUpdate(playerEntity.HP);
	}

	public override void OnBack()
	{

	}

	public override void Draw()
	{
		if (GameplayManager.Enabled)
		{
			var gt = gameplayState.GetManager<GameplayManager>();
			float norm = 1.0f - gt.NormalizedTime;

			const int totalSeconds = 6 * 60 * 60;
			int elapsedSec = (int)(norm * totalSeconds);
			TimeSpan ts = TimeSpan.FromSeconds(elapsedSec);

			string timeString = ts.ToString(@"hh\:mm");

			references["time-text"].Text = timeString;
		}
		else
		{
			references["time-text"].Text = "--:--";
		}

		base.Draw();
	}

	private void OnHPUpdate(int amt)
	{
		references["hp-text"].Text = $"HP: {amt}/{playerEntity.MaxHP}";
	}

	private void OnWeaponUpdate(Weapon w)
	{
		references["ammo-text"].Text = w.UsesAmmo ? $"{w.Name} ({w.CurrentAmmo}/{w.CurrentMaxAmmo})" : $"{w.Name}";
	}

	private void UpdateVisibility()
	{
		var showHUD = GameplayManager.Enabled;
		var showDialogue = gameplayState.GetManager<DialogueManager>().CurrentDialogue != null;

		foreach (var i in elements)
		{
			if (i.ID == "dialogue-text")
			{
				i.Visible = showDialogue;
			}
			else
			{
				i.Visible = !showDialogue && showHUD;
			}
		}
	}
}