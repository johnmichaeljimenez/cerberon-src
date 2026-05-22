using Main.Core;

namespace Main.Gameplay.Entities.Player;

public interface IInteractable
{
	bool IsDestroyed { get; }
	bool Interactable { get; }
	Vector2 Position { get; set; }

	bool Interact();
}

public class PlayerInteraction : EntityModule<PlayerEntity>
{
	[DataConfig(1f)]
	public static float RADIUS_MOUSE;
	[DataConfig(5f)]
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

		var maxDist = RADIUS_MOUSE;
		IInteractable nearest = null;
		var mousePos = InputManager.MouseWorldPosition;
		foreach (var i in gameplayState.CurrentWorld.GetEntitiesByGroup(nameof(IInteractable)))
		{
			if (i.IsDestroyed || i is not IInteractable interactable)
				continue;

			var dPlayer = (i.Position - Entity.Position).Length();
			if (dPlayer > RADIUS_PLAYER)
				continue;

			var dir = i.Position - mousePos;
			var dist = dir.Length();

			if (dist > maxDist)
				continue;

			maxDist = dist;
			nearest = interactable;
		}

		if (current != nearest)
		{
			current = nearest;
			OnInteractableChanged.Publish(current);
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
				current = null;
				OnInteractableChanged.Publish(null);
			}
		}
	}
}