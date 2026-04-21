using Main.Core;
using Main.Gameplay.Entities;

namespace Main.Gameplay.Managers;

public class GameplayManager : BaseManager
{
	public float GameTime => _gameTime;
	private float _gameTime;
	public readonly float MaxGameTime = 60f; //temporary hardcoded

	private bool started;

	public GameplayManager(GameplayState gameplayState) : base(gameplayState)
	{
		gameplayState.GetManager<PlayerManager>().OnPlayerDeath.Subscribe(OnPlayerDeath).AddTo(disposables);
	}

	private void End(bool win)
	{
		started = false;
		Log.Send(win? "You win" : "You lose");
		PauseHandler.Pause("ending");
	}

	private void OnPlayerDeath(PlayerEntity entity)
	{
		End(false);
	}

	public override void Init()
	{
		base.Init();
		started = true;
		_gameTime = MaxGameTime;
	}

	public override void Dispose()
	{
		base.Dispose();
		started = false;
	}

	public override void Update(float dt, float udt)
	{
		base.Update(dt, udt);

		if (started)
		{
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