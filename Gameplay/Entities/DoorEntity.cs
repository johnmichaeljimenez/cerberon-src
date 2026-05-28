using System.ComponentModel;
using Main.Core;
using Main.Effects;
using Main.Gameplay.Entities.Player;
using Main.Gameplay.Level;
using Main.Gameplay.Managers;

namespace Main.Gameplay.Entities;

public class DoorEntity : BaseEntity, IWaypointModifier, IInteractable
{
	private readonly Vector2 DEFAULT_SIZE = new Vector2(3, 1);

	[JsonProperty]
	public float Rotation { get; set; }

	[JsonProperty]
	public Vector2 Size { get; set; }

	[JsonProperty]
	[DefaultValue(true)]
	public bool Interactable { get; set; }

	[JsonIgnore]
	public bool IsOpen { get; set; }

	[JsonIgnore]
	public WaypointManager.Node Node { get; set; }

	[JsonIgnore]
	public List<Wall> Colliders { get; set; } = new();

	public InteractionType InteractionType => InteractionType.Use;
	private Shadow shadow;

	public override void Init(GameplayState gameplayState)
	{
		base.Init(gameplayState);
		Groups.Add(nameof(IInteractable));

		var size = Size;
		if (MathF.Abs(size.X) <= 0.01f)
			size.X = DEFAULT_SIZE.X;
		if (MathF.Abs(size.Y) <= 0.01f)
			size.Y = DEFAULT_SIZE.Y;

		Size = size;

		gameplayState.GetManager<CollisionManager>().AddWalls(Position, Size, Colliders, Wall.WallFlags.DrawOverlay, CollisionHeight.High, false, Rotation);
		shadow = LightingSystem.AddShadow(Position, Size, Rotation);
	}

	public override void Dispose()
	{
		base.Dispose();

		Colliders.ForEach(p => gameplayState.GetManager<CollisionManager>().RemoveWall(p));
		if (shadow != null)
			LightingSystem.RemoveShadow(shadow);
	}

	protected override void OnActiveStateChanged(bool isActive)
	{
		base.OnActiveStateChanged(isActive);
		SetState();
	}

	public void SetOpen(bool isOpen)
	{
		IsOpen = isOpen;
		SetState();
	}

	private void SetState()
	{
		Node.Enabled = IsOpen || !IsActive;

		shadow.Enabled = !IsOpen && IsActive;
		foreach (var i in Colliders)
		{
			i.Enabled = !IsOpen && IsActive;
		}
	}

	public bool Interact()
	{
		SetOpen(!IsOpen);
		return true;
	}
}