using System.ComponentModel;
using Cerberon.Gameplay.Entities.Player;

namespace Cerberon.Gameplay.Entities;

public class SwitchEntity : BaseEntity, IInteractable
{
	[JsonProperty]
	public bool OneShot { get; set; }
	[JsonProperty]
	public bool InitialState { get; set; }
	
	[JsonProperty]
	[DefaultValue(true)]
	public bool Interactable { get; set; }

	private bool triggeredOneShot;

	public InteractionType InteractionType => InteractionType.Use;

	public bool Interact()
	{
		if (OneShot && triggeredOneShot)
			return false;

		InitialState = !InitialState;
		return true;
	}
}