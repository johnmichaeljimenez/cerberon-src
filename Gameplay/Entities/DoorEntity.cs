using System.ComponentModel;
using Cerberon.Core;
using Cerberon.Effects;
using Cerberon.Gameplay.Entities.Player;
using Cerberon.Gameplay.Level;
using Cerberon.Gameplay.Managers;
using Cerberon.Helpers;
using Tween;

namespace Cerberon.Gameplay.Entities;

public class DoorEntity : BaseEntity, IInteractable, IWaypointModifier, ICombatEntity
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

	public float HurtboxRadius => 2f;

	private Shadow shadow;  //there was a current bug in lighting system where static (one shot shadow) lights in level preserve the original shadow of this door, and I decided to not fix it as it looks like it renders the doorframe even for a 2d topdown view.

	private float lifeCooldown; //if a door is closed, enemies can bash it repeatedly until it opens, and player cannot close it immediately during certain cooldown
	private int hitCount;

	private Vector2 initialPos;
	private Vector2 doorPos;
	private string tweenID;
	private Vector2 slideDirection;
	public bool IsDead { get; private set; }

	public override void Init(GameplayState gameplayState)
	{
		base.Init(gameplayState);
		Groups.Add(nameof(IInteractable));
		Groups.Add(nameof(ICombatEntity));
		IsDead = false;

		hitCount = 3;
		lifeCooldown = 5;
		tweenID = $"DoorSlide#{ID}";
		initialPos = Position;
		doorPos = initialPos;

		var targetAngle = (Rotation + 180) * MathF.PI / 180f;
		slideDirection = new Vector2(
			MathF.Cos(targetAngle),
			MathF.Sin(targetAngle)
		);

		var size = Size;
		if (MathF.Abs(size.X) <= 0.01f)
			size.X = DEFAULT_SIZE.X;
		if (MathF.Abs(size.Y) <= 0.01f)
			size.Y = DEFAULT_SIZE.Y;

		Size = size;

		gameplayState.GetManager<CollisionManager>().AddWalls(Position, Size, Colliders, Wall.WallFlags.DrawOverlay, CollisionHeight.High, false, Rotation);
		shadow = LightingSystem.AddShadow(Position, Size, Rotation);

		foreach (var i  in Colliders)
		{
			i.Entity = this;
		}
	}

	public override void Dispose()
	{
		base.Dispose();

		TweenManager.ClearByPrefix(tweenID);
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
		if (IsOpen == isOpen)
			return;

		IsOpen = isOpen;
		SetState();

		if (IsActive)
		{
			var openPos = doorPos + (slideDirection * 2.5f);

			TweenManager.Add(new Tween<Vector2>(
				() => doorPos,
				pos =>
				{
					var delta = pos - doorPos;
					doorPos = pos;

					shadow.Move(delta);
					foreach (var i in Colliders)
					{
						i.From += delta;
						i.To += delta;
						i.Recalculate();
					}
				},
				IsOpen ? openPos : initialPos,
				IsOpen && hitCount <= 0? 0.1f : 0.5f, null, tweenID).SetEasing(Easing.QuadInOut));
		}
	}

	private void SetState()
	{
		Node.Enabled = IsOpen || Interactable || !IsActive;
		Node.NodeFlags = !IsOpen && IsActive ? WaypointManager.NodeFlags.Blocked : WaypointManager.NodeFlags.None;

		shadow.Enabled = IsActive;
		foreach (var i in Colliders)
		{
			i.Enabled = IsActive;
		}
	}

	public bool Interact()
	{
		if (hitCount <= 0)
			return false;

		SetOpen(!IsOpen);
		return true;
	}

	public override void Update(float dt, float udt)
	{
		base.Update(dt, udt);

		if (hitCount <= 0)
		{
			if (Utils.Countdown(ref lifeCooldown, dt))
			{
				IsDead = false;
				hitCount = 3;
			}
		}
	}

	public void OnNodeAdded(WaypointManager.Node node)
	{
		node.NodeFlags = WaypointManager.NodeFlags.Blocked;
	}

	public bool ApplyDamage(int amt, CharacterEntity from)
	{
		if (hitCount <= 0 || !Interactable || IsOpen)
			return false;

		hitCount -= 1;
		OnHit(1, hitCount == 0, from);
		if (hitCount == 0)
		{
			IsDead = true;
			SetOpen(true);
			lifeCooldown = 5;

			OnDeath();
		}

		return true;
	}

	public void OnHit(float amt, bool isDead, CharacterEntity from)
	{
		Game.Instance.Camera.Shake(0.8f, null);
		AudioHandler.PlaySound("knock-slam", Position);
	}

	public void OnDeath()
	{
		Game.Instance.Camera.Shake(3f, null);
		AudioHandler.PlaySound("break", Position);
	}
}