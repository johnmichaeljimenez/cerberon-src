using System.Diagnostics;
using Cerberon.Core;
using Cerberon.Effects;
using Cerberon.Gameplay.Entities;
using Cerberon.Gameplay.Entities.Player;
using Cerberon.Gameplay.Managers;
using Cerberon.Helpers;

namespace Cerberon.Gameplay.Events;

public class EventSetup : IDisposable
{
	private readonly List<IDisposable> disposables = new();
	private GameplayState gameplayState;
	private GameplayManager gameplayManager;
	private GameplayEventManager gameplayEventManager;

	public void Setup(GameplayState state)
	{
		gameplayState = state;
		gameplayManager = state.GetManager<GameplayManager>();
		gameplayEventManager = state.GetManager<GameplayEventManager>();

		gameplayManager.OnTimeEnd.Subscribe(_ =>
		{
			TimeEnd();
		}).AddTo(disposables);

		state.GetManager<TriggerManager>().OnTriggerExecute.Subscribe(t =>
		{
			switch (t.Item2.TriggerID)
			{
				case nameof(StartFight):
					StartFight();
					break;
				case nameof(PowerOff):
					PowerOff();
					break;
				case nameof(Ending):
					Ending();
					break;
				default:
					return;
			}
		}).AddTo(disposables);
	}

	private void TimeEnd()
	{
		gameplayEventManager.RunEvent("power",
			new PlayAudio("phone", null, true),
			new Wait(0.1f),
			new PlayAudio("phone", null, true),
			new ShowDialogue("power-1", false),
			new Wait(0.5f),
			new Exec(() => gameplayState.GetManager<TriggerManager>().Find(nameof(PowerOff))[0].Enabled = true)
		);
	}

	private void PowerOff()
	{
		gameplayEventManager.RunEvent("power",	//can be intentionally overriden since player can wait at the gate already before time runs out
			new SetLightGroupState("Main", false),
			new Wait(0.1f),
			new SetLightGroupState("Main", true),
			new Wait(0.1f),
			new SetLightGroupState("Main", false),
			new Wait(0.1f),
			new SetLightGroupState("Main", true),
			new Wait(0.1f),
			new SetLightGroupState("Main", false),
			new ShowDialogue("power-2", false)
		);
	}

	private void PowerOn()
	{
		gameplayEventManager.RunEvent("power-on",
			new Exec(() => Game.Instance.Camera.Shake(3f, null)),
			new SetLightGroupState("Main", false),
			new Wait(0.1f),
			new SetLightGroupState("Main", true),
			new Wait(0.1f),
			new SetLightGroupState("Main", false),
			new Wait(0.1f),
			new SetLightGroupState("Main", true),
			new Exec(() => gameplayState.GetManager<TriggerManager>().Find(nameof(Ending))[0].Enabled = true)
		);
	}

	private void Ending()
	{
		gameplayEventManager.RunEvent("end-1",
			new ShowDialogue("end-1", true),
			new Wait(0.5f),
			new Exec(() =>
			{
				gameplayState.GetManager<GameplayManager>().End(true);
			})
		);
	}

	public void Dispose()
	{
		disposables.ForEach(p => p.Dispose());
	}

	private void StartFight()
	{
		var player = gameplayManager.PlayerCharacter;
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
			new SetLightGroupState("<default>", false),
			new PlayAudio("knock-slam", sfxPosition),
			new Wait(0.2f),
			new PlayAudio("knock-slam", sfxPosition),
			new Wait(0.2f),
			new PlayAudio("knock-slam", sfxPosition),
			new Wait(0.1f),

			new Exec(() => door.SetActive(false)),
			new SetLightGroupState("<default>", true),
			new PlayAudio("break", sfxPosition),
			new Exec(() => Game.Instance.Camera.Shake(3f, null)),
			new SpawnEnemy(sfxPosition ?? Vector2.Zero, 1f),
			new Wait(0.2f),

			new ShowDialogue("intro-2-start", false),
			new Wait(0.5f),
			new SpawnEnemy(sfxPosition ?? Vector2.Zero, 0.8f),
			new Wait(0.2f),
			new SpawnEnemy(sfxPosition ?? Vector2.Zero, 0.8f),
			new Wait(1f),

			new PlayAudio("phone", null, true),
			new Wait(0.1f),
			new PlayAudio("phone", null, true),
			new ShowDialogue("intro-2", false),
			new Wait(0.1f),
			new Exec(() =>
			{
				gameplayState.GetManager<GameplayManager>().Begin();
			})
		);

		var powerSwitch = gameplayState.CurrentWorld.GetEntitiesByNameTag<SwitchEntity>("PowerSwitch");
		player.GetModule<PlayerInteraction>().OnInteract.Subscribe(e =>
		{
			if (e != powerSwitch)
				return;

			PowerOn();
		}).AddTo(disposables);
	}
}