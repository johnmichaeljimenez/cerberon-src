using Cerberon.Core;
using Cerberon.Effects;
using Cerberon.Gameplay.Level;
using Cerberon.Gameplay.Managers;
using Cerberon.Helpers;
using static Cerberon.Gameplay.Managers.WaypointManager;

namespace Cerberon.Gameplay.Entities;

public class PropEntity : BaseEntity
{
	[JsonProperty]
	public float Rotation { get; set; }

	[JsonProperty]
	public Vector2 ColliderSize { get; set; }

	[JsonIgnore]
	public List<Wall> Colliders { get; set; } = new();

	private readonly List<Node> nodes = new();

	public override void Init(GameplayState gameplayState)
	{
		base.Init(gameplayState);

		gameplayState.GetManager<CollisionManager>().AddWalls(Position, ColliderSize, Colliders, Wall.WallFlags.DrawOverlay, CollisionHeight.Low, false, Rotation);

		foreach (var i in Colliders)
		{
			i.Entity = this;
			i.Enabled = false;	//do not interfere with waypoint generation
		}
	}

	public override void PostInit()
	{
		base.PostInit();

		nodes.AddRange(gameplayState.GetManager<WaypointManager>().GetNodesInsideRect(Position, ColliderSize, Rotation));
		UpdateSpace();
	}

	private void UpdateSpace()
	{
		foreach (var i in nodes)
		{
			i.Enabled = !IsActive;
		}

		foreach (var i in Colliders)
		{
			i.Enabled = IsActive;
		}
	}

	protected override void OnActiveStateChanged(bool isActive)
	{
		base.OnActiveStateChanged(isActive);
		UpdateSpace();
	}

	public override void Dispose()
	{
		base.Dispose();

		Colliders.ForEach(p => gameplayState.GetManager<CollisionManager>().RemoveWall(p));
	}

	public override void Draw()
	{
		CurrentSprite?.Draw(Position, 1, Rotation);
	}
}