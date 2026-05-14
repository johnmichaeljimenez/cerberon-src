using Main.Gameplay.Managers;
using Main.Helpers;

namespace Main.Gameplay.Events;

public class EventSetup : IDisposable
{
	private readonly List<IDisposable> disposables = new();
	private GameplayState gameplayState;
	private GameplayEventManager gameplayEventManager;

	public void Setup(GameplayState state, GameplayEventManager ev)
	{
		gameplayState = state;
		gameplayEventManager = ev;

		//start of game
		state.GetManager<GameplayManager>().OnGameStart.Subscribe(_ =>
		{
			// ev.RunEvent("start");
		}).AddTo(disposables);

		//start of game
		state.GetManager<GameplayManager>().OnFightEnd.Subscribe(_ =>
		{
			state.GetManager<TriggerManager>().Find(nameof(Ending))[0].Enabled = true;
		}).AddTo(disposables);

		//start fight
		state.GetManager<TriggerManager>().OnTriggerExecute.Subscribe(t =>
		{
			switch (t.Item2.TriggerID)
			{
				case nameof(StartFight):
					StartFight();
					break;
				case nameof(Ending):
					StartFight();
					break;
				default:
					return;
			}
		});
	}

	public void Dispose()
	{
		disposables.ForEach(p => p.Dispose());
	}

	private void StartFight()
	{
		gameplayEventManager.RunEvent("startfight",
			new PlayAudio("break", null),
			new Wait(0.3f),
			new PlayAudio("break", null),
			new Wait(0.3f),
			new PlayAudio("break", null),
			new Wait(0.3f),
			new PlayAudio("break", null),
			new Wait(0.1f),
			new Exec(() =>
			{
				gameplayState.GetManager<GameplayManager>().Begin();
			})
		);
	}

	private void Ending()
	{
		gameplayState.GetManager<GameplayManager>().End(true);
	}
}