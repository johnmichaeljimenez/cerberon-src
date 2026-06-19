using Cerberon.Core;
using Cerberon.Effects;
using Cerberon.Gameplay.Entities.Player;
using Cerberon.Gameplay.Events;
using Cerberon.Helpers;
using Cerberon.UI;

namespace Cerberon.Gameplay.Managers;

public class GameplayManager : BaseManager
{

	[DataConfig(defaultValue: true)]
	public static bool Enabled;

	[DataConfig]
	public static List<Color> AmbientGradient = new()
	{
		new(20, 20, 34),
		new(34, 34, 34),
		new(20, 20, 20),
		new(34, 34, 34),
		new(60, 34, 34),
		new(20, 20, 20),
		new(12, 12, 12),
		new(20, 20, 20),
		new(34, 34, 34),
		new(34, 34, 34),
		new(80, 80, 50),
		new(80, 80, 50),
		new(140, 140, 120)
	};

	[DataConfig]
	public static float MaxGameTime = 300f;

	public float GameTime => _gameTime;
	private float _gameTime;

	public bool Running { get; private set; }
	public float NormalizedTime { get; internal set; }


	private EventSetup events = new();

	public PlayerEntity PlayerCharacter { get; private set; }
	public readonly Signal<PlayerEntity> OnPlayerDeath = new();
	public readonly Signal<Unit> OnGameStart = new();
	public readonly Signal<Unit> OnFightStart = new();
	public readonly Signal<int> OnTimeTick = new();
	public readonly Signal<Unit> OnTimeEnd = new();

	private float accumulator;

	public GameplayManager(GameplayState gameplayState) : base(gameplayState)
	{

	}

	public void End(bool win)
	{
		Running = false;
		Log.Send(win ? "You win" : "You lose");
		UIManager.ShowScreen<EndScreen>((gameplayState, win), false);
		PauseHandler.Pause("ending");
	}

	public void PlayerDead(PlayerEntity entity)
	{
		End(false);
	}

	public override void Init()
	{
		base.Init();
		Running = Enabled;
		_gameTime = MaxGameTime;

		events.Setup(gameplayState);
	}

	public override void OnEnter()
	{
		base.OnEnter();
		
		LightingSystem.AmbientLightColor = AmbientGradient[0];
		OnGameStart.Publish(Unit.Default);
	}

	public override void Dispose()
	{
		base.Dispose();
		events.Dispose();
		Running = false;
	}

	public override void Update(float dt, float udt)
	{
		base.Update(dt, udt);

		if (Running)
		{
			NormalizedTime = _gameTime / MaxGameTime;
			LightingSystem.AmbientLightColor = AmbientGradient.LerpGradient(1.0f - NormalizedTime);

			accumulator += dt;
			if (accumulator >= 1.0f)
			{
				accumulator = 0;

				var t = (int)MathF.Ceiling(MaxGameTime - _gameTime);
				OnTimeTick.Publish(t);
			}

			if (Utils.Countdown(ref _gameTime, dt))
			{
				OnTimeEnd.Publish(Unit.Default);
				Running = false;
			}
		}
	}

	public override void DrawImGui()
	{
		base.DrawImGui();

		var n = (int)_gameTime;
		ImGui.SliderInt($"Game Time", ref n, 0, (int)MaxGameTime);
	}
	
	public void SpawnPlayer(Vector2 position)
	{
		PlayerCharacter = gameplayState.CurrentWorld.SpawnEntity<PlayerEntity>((e) =>
		{
			e.Position = position;
		});
	}

	public void Begin()
	{
		Enabled = true;
		Running = true;
		OnFightStart?.Publish(Unit.Default);
	}
}