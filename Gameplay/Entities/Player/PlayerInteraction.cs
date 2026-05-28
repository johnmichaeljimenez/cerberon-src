using Main.Core;

namespace Main.Gameplay.Entities.Player;

public enum InteractionType
{
	Use,
	Pickup
}

public interface IInteractable
{
	InteractionType InteractionType { get; }
	bool IsDestroyed { get; }
	bool Interactable { get; }
	Vector2 Position { get; set; }

	bool Interact();
}

public class PlayerInteraction : EntityModule<PlayerEntity>
{
	[DataConfig(3f)]
	public static float RADIUS_PLAYER;

	private IInteractable current;

	public readonly Signal<IInteractable> OnInteractableChanged = new();
	public readonly Signal<IInteractable> OnInteract = new();

	public PlayerInteraction(GameplayState gameplayState, PlayerEntity playerEntity) : base(gameplayState, playerEntity)
	{

	}

	public override void Update(float dt, float udt)
	{
		base.Update(dt, udt);

		var maxDist = RADIUS_PLAYER;
		IInteractable nearest = null;
		var mousePos = InputManager.MouseWorldPosition;
		foreach (var i in gameplayState.CurrentWorld.GetEntitiesByGroup(nameof(IInteractable)))
		{
			if (i.IsDestroyed || i is not IInteractable interactable)
				continue;

			if (!interactable.Interactable)
				continue;

			var dPlayer = (i.Position - Entity.Position).Length();
			if (dPlayer > RADIUS_PLAYER)
				continue;

			var dir = i.Position - mousePos;
			var dist = dir.Length();
			if (dPlayer < dist)
				dPlayer = dist;

			if (dist > maxDist)
				continue;

			maxDist = dist;
			nearest = interactable;
		}

		if (current != nearest)
		{
			SetCurrent(nearest);
		}

		if (current != null && InputManager.IsPressed(InputAction.Interact))
		{
			if (current.Interact())
				OnInteract.Publish(current);
		}
	}

	public override void LateUpdate(float dt, float udt)
	{
		base.LateUpdate(dt, udt);

		if (current != null)
		{
			if (current.IsDestroyed)
			{
				SetCurrent(null);
			}
		}
	}

	private void SetCurrent(IInteractable current)
	{
		this.current = current;
		OnInteractableChanged.Publish(current);

		if (current != null)
			InputManager.SetCursorState("interact", CursorType.Interaction);
		else
			InputManager.RemoveCursorState("interact");
	}
}