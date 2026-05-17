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

		//end of game
		state.GetManager<GameplayManager>().OnTimeEnd.Subscribe(_ =>
		{
			gameplayEventManager.RunEvent("end-1",
				new ShowDialogue("end-1", true)
			);

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
					Ending();
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
			new PlayAudio("knock-slam", null),
			new Wait(0.2f),
			new PlayAudio("knock-slam", null),
			new Wait(0.5f),
			new ShowDialogue("intro-1", true),
			new Wait(0.5f),
			new PlayAudio("knock-slam", null),
			new Wait(0.2f),
			new PlayAudio("knock-slam", null),
			new Wait(0.2f),
			new PlayAudio("knock-slam", null),
			new Wait(0.1f),
			new PlayAudio("break", null),
			new Wait(1f),
			new ShowDialogue("intro-2", false),
			new Wait(0.5f),
			new Exec(() =>
			{
				gameplayState.GetManager<GameplayManager>().Begin();
			})
		);
	}

	private void Ending()
	{
		gameplayEventManager.RunEvent("end-2",
			new ShowDialogue("end-2", true),
			new Wait(0.5f),
			new Exec(() =>
			{
				gameplayState.GetManager<GameplayManager>().End(true);
			})
		);
	}
}