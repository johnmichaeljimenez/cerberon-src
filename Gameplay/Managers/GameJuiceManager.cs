using Main.Core;
using Main.Gameplay.Entities.Player;
using Main.Helpers;
using Tween;

namespace Main.Gameplay.Managers;

public class GameJuiceManager : BaseManager
{
	[DataConfig(-2)]
	public static float ZoomOut;
	[DataConfig(12)]
	public static float ZoomIn;
	[DataConfig(20)]
	public static float ZoomClearance;
	[DataConfig(0.5f)]
	public static float ZoomWeight;

	private float currentZoom;
	private float zoomClearance;

	private PlayerEntity player;

	public GameJuiceManager(GameplayState gameplayState) : base(gameplayState)
	{

	}

	public override void OnEnter()
	{
		base.OnEnter();

		player = gameplayState.GetManager<GameplayManager>().PlayerCharacter;

		player.OnHealItemUpdate.Subscribe(p => { OnHeal(p.Item1); }).AddTo(disposables);
		player.OnHealUse.Subscribe(_ => { OnHealUse(_); }).AddTo(disposables);
		player.OnPositionChanged.Subscribe(OnPlayerPositionChanged).AddTo(disposables);
	}

	public override void Update(float dt, float udt)
	{
		base.Update(dt, udt);

		//claustrophobic effect by making the camera zoom in if player is in a low-clearance node region (which means a lot of walls nearby like rooms or corridors)
		//likewise, apply cinematic effect (real bird-eye view) when in an open area (ex. outdoors)
		currentZoom = Raymath.Lerp(currentZoom, zoomClearance, dt);
		var z = Raymath.Clamp01(currentZoom / ZoomClearance);
		Game.Instance.Camera.Zoom.SetModifier("clearance", Raymath.Lerp(ZoomIn, ZoomOut, z)); //TODO: dataconfig
	}

	private void OnPlayerPositionChanged(Vector2 vector)
	{
		if (player.PreviousNode == null || player.NearestNode == null)
		{
			zoomClearance = 0;
			return;
		}

		zoomClearance = (player.PreviousNode.ClearanceWeighted + player.NearestNode.ClearanceWeighted) * 0.5f;
		zoomClearance *= ZoomWeight;
	}

	private void OnHealUse(Unit _)
	{
		Game.Instance.Camera.Shake(5, null);
	}

	private void OnHeal(bool healing)
	{
		var cam = Game.Instance.Camera;
		var a = 0;
		var b = 1;
		var to = healing ? b : a;
		var duration = healing ? 0.4f : 0.7f;

		TweenManager.Add(
			new Tween<float>(
				() => cam.Zoom.GetModifier("heal"),
				p => cam.Zoom.SetModifier("heal", p),
				to,
				duration, null, "CameraZoom")
			.SetEasing(Easing.QuadInOut)
		);
	}

	public override void DrawImGui()
	{
		base.DrawImGui();

		var z = Raymath.Clamp01(currentZoom / ZoomClearance);
		ImGui.Text($"Current zoom: {currentZoom:F1}");
		ImGui.Text($"Zoom clearance: {Raymath.Lerp(ZoomIn, ZoomOut, z):F1}");
	}
}