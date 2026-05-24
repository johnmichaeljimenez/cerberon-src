using Main.Core;
using Main.Gameplay.Entities.Player;
using Main.Helpers;
using Tween;

namespace Main.Gameplay.Managers;

public class GameJuiceManager : BaseManager
{
	public GameJuiceManager(GameplayState gameplayState) : base(gameplayState)
	{

	}

	public override void OnEnter()
	{
		base.OnEnter();

		gameplayState.GetManager<GameplayManager>().PlayerCharacter.OnHealItemUpdate.Subscribe(p => { OnHeal(p.Item1); }).AddTo(disposables);
		gameplayState.GetManager<GameplayManager>().PlayerCharacter.OnHealUse.Subscribe(_ => { OnHealUse(_); }).AddTo(disposables);
	}

	private void OnHealUse(Unit _)
	{
		Game.Instance.Camera.Shake(5, null);
	}

	private void OnHeal(bool healing)
	{
		var cam = Game.Instance.Camera;
		var a = CameraController.DefaultZoom;
		var b = CameraController.DefaultZoom + 1;
		var to = healing? b : a;
		var duration = healing? 0.4f : 0.7f;

		TweenManager.Add(
			new Tween<float>(
				() => cam.Camera.Zoom, 
				p => cam.Camera.Zoom = p, 
				to, 
				duration, null, "CameraZoom")
			.SetEasing(Easing.QuadInOut)
		);
	}
}