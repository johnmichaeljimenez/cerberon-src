using Main.Core;
using Main.Gameplay.Entities.Player;
using Main.Helpers;
using Main.UI;

namespace Main.Gameplay.Managers;

public class GameplayManager : BaseManager
{
	public float GameTime => _gameTime;
	private float _gameTime;
	public readonly float MaxGameTime = 180f; //temporary hardcoded

	public bool Running { get; private set; }
	public float NormalizedTime { get; internal set; }

	public GameplayManager(GameplayState gameplayState) : base(gameplayState)
	{
		gameplayState.GetManager<PlayerManager>().OnPlayerDeath.Subscribe(OnPlayerDeath).AddTo(disposables);
	}

	private void End(bool win)
	{
		Running = false;
		Log.Send(win ? "You win" : "You lose");
		UIManager.ShowScreen<EndScreen>((gameplayState, win), false);
		PauseHandler.Pause("ending");
	}

	private void OnPlayerDeath(PlayerEntity entity)
	{
		End(false);
	}

	public override void Init()
	{
		base.Init();
		Running = true;
		_gameTime = MaxGameTime;
	}

	public override void Dispose()
	{
		base.Dispose();
		Running = false;
	}

	public override void Update(float dt, float udt)
	{
		base.Update(dt, udt);

		if (Running)
		{
			NormalizedTime = _gameTime / MaxGameTime;
			if (Utils.Countdown(ref _gameTime, dt))
			{
				End(true);
			}
		}
	}

	public override void DrawImGui()
	{
		base.DrawImGui();

		var n = (int)_gameTime;
		ImGui.SliderInt($"Game Time", ref n, 0, (int)MaxGameTime);
	}
}