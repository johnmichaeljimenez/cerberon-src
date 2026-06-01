using Cerberon.Core;
using Cerberon.Gameplay.Entities;
using Cerberon.Gameplay.Managers;
using Cerberon.Helpers;

namespace Cerberon.Gameplay.Events;

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
		var door = gameplayState.CurrentWorld.GetEntityByNameTag<DoorEntity>("intro-door");
		var mk = gameplayState.CurrentWorld.FindMarkerPosition("intro-sfx");
		Vector2? sfxPosition = mk == null ? null : mk.Position;

		gameplayEventManager.RunEvent("startfight",
			new Exec(() => Game.Instance.Camera.Shake(0.8f, null)),
			new PlayAudio("knock-slam", sfxPosition),
			new Wait(0.2f),
			new PlayAudio("knock-slam", sfxPosition),
			new Wait(0.5f),
			new ShowDialogue("intro-1", true),
			new Wait(0.5f),
			new Exec(() => Game.Instance.Camera.Shake(0.8f, null)),
			new PlayAudio("knock-slam", sfxPosition),
			new Wait(0.2f),
			new PlayAudio("knock-slam", sfxPosition),
			new Wait(0.2f),
			new PlayAudio("knock-slam", sfxPosition),
			new Wait(0.1f),
			new Exec(() => door.SetActive(false)),
			new PlayAudio("break", sfxPosition),
			new Exec(() => Game.Instance.Camera.Shake(3f, null)),
			new SpawnEnemy(sfxPosition ?? Vector2.Zero, 1f),
			new Wait(0.2f),
			new ShowDialogue("intro-2", false),
			new Wait(0.5f),
			new SpawnEnemy(sfxPosition ?? Vector2.Zero, 0.8f),
			new Wait(0.2f),
			new SpawnEnemy(sfxPosition ?? Vector2.Zero, 0.8f),
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