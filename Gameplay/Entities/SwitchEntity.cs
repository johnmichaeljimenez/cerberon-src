using System.ComponentModel;
using Cerberon.Core;
using Cerberon.Gameplay.Entities.Player;

namespace Cerberon.Gameplay.Entities;

public class SwitchEntity : BaseEntity, IInteractable
{
	[JsonProperty]
	public bool OneShot { get; set; }
	[JsonProperty]
	public bool InitialState { get; set; }
	[JsonProperty]
	public string SpriteNameEnabledState { get; set; }
	[JsonProperty]
	public string SpriteNameDisabledState { get; set; }

	[JsonProperty]
	[DefaultValue(true)]
	public bool Interactable { get; set; }

	private bool triggeredOneShot;
	private Sprite spriteEnabled, spriteDisabled;

	public InteractionType InteractionType => InteractionType.Use;

	public override void Init(GameplayState gameplayState)
	{
		Groups.Add(nameof(IInteractable));
		
		if (!string.IsNullOrWhiteSpace(SpriteNameEnabledState))
			spriteEnabled = AssetManager.GetSprite(SpriteNameEnabledState);

		if (!string.IsNullOrWhiteSpace(SpriteNameDisabledState))
			spriteDisabled = AssetManager.GetSprite(SpriteNameDisabledState);

		base.Init(gameplayState);
	}

	public bool Interact()
	{
		if (OneShot && triggeredOneShot)
			return false;

		InitialState = !InitialState;
		AudioHandler.PlaySound("generic/button-press", Position, overrideID: $"Switch#{ID}");
		return true;
	}

	public override void Draw()
	{
		//fully override base 
		var sprite = CurrentSprite;
		if (InitialState && spriteEnabled != null)
			sprite = spriteEnabled;
		else if (!InitialState && spriteDisabled != null)
			sprite = spriteDisabled;

		sprite?.Draw(Position);
	}
}