using Main.Gameplay.Managers;
using Main.Helpers;

namespace Main.Gameplay.Events;

public class EventSetup : IDisposable
{
	private readonly List<IDisposable> disposables = new();

	public void Setup(GameplayState state,  GameplayEventManager ev)
	{
		state.GetManager<GameplayManager>().OnGameStart.Subscribe(_ => {
			ev.RunEvent("start", 
				new Exec(() => state.GetManager<AIDirectorManager>().Paused = true),
				new Wait(5),
				new PlayAudio("break", null),
				new Wait(0.3f),
				new PlayAudio("break", null),
				new Wait(0.3f),
				new PlayAudio("break", null),
				new Wait(0.3f),
				new PlayAudio("break", null),
				new Wait(0.1f),
				new Exec(() => state.GetManager<AIDirectorManager>().Begin())
			);
		}).AddTo(disposables);
	}

	public void Dispose()
	{
		disposables.ForEach(p => p.Dispose());
	}
}