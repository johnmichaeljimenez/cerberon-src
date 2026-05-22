using Main.Core;
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

	private IInteractable currentInteractable;

	public HUDScreen(object context) : base(context)
	{
		gameplayState = context as GameplayState;
		gameplayState.GetManager<GameplayManager>().OnFightStart.Subscribe(_ => UpdateVisibility()).AddTo(disposables);
		gameplayState.GetManager<DialogueManager>().OnDialogueShow.Subscribe(_ => UpdateVisibility()).AddTo(disposables);

		playerEntity = gameplayState.GetManager<GameplayManager>().PlayerCharacter;
		playerEntity.OnHPChanged.Subscribe(OnHPUpdate).AddTo(disposables);
		playerEntity.GetModule<PlayerInteraction>().OnInteractableChanged.Subscribe(OnInteractableChanged).AddTo(disposables);

		weapons = playerEntity.Weapons;
		weapons.OnWeaponAmmoChanged.Subscribe(OnWeaponUpdate).AddTo(disposables);
		weapons.OnWeaponSelected.Subscribe(OnWeaponUpdate).AddTo(disposables);
	}

	private void OnInteractableChanged(IInteractable interactable)
	{
		currentInteractable = interactable;
		references["interact-text"].Visible = currentInteractable != null;
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

		if (currentInteractable != null)
		{
			var pos = Vector2.Lerp(currentInteractable.Position, InputManager.MouseWorldPosition, 0.25f);
			pos = Game.Instance.Camera.WorldToScreen(pos);

			var el = references["interact-text"];
			el.Position = pos;
		}

		base.Draw();
	}

	public override void UpdateElements(List<UIElement> elements)
	{
		base.UpdateElements(elements);
		UpdateVisibility();
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
		var dialogue = gameplayState.GetManager<DialogueManager>().CurrentDialogue;

		foreach (var i in elements)
		{
			if (i.ID == "interact-text")
				continue;

			if (i.ID == "dialogue-text")
			{
				i.Visible = dialogue != null;
				if (dialogue != null)
				{
					i.Text = $"[{dialogue.CharacterID}] {dialogue.Message}";
				}
			}
			else
			{
				i.Visible = showHUD;
			}
		}
	}
}