using Cerberon.Core;
using Cerberon.Effects;
using Cerberon.Gameplay.Entities;
using Cerberon.Gameplay.Managers;
using OpcodeEngine.Commands;
using OpcodeEngine.Core;

namespace Cerberon.Gameplay.Events;

public abstract class GameCommand : Command
{
	protected ScriptRunner Context => (ScriptRunner)Engine;
	protected GameplayState gameplayState => Context.gameplayState;
}

public class FadeIn : GameCommand
{
	public override void OnEnter()
	{
		FadeHandler.FadeIn();
	}

	public override bool OnTick(float dt)
	{
		return !FadeHandler.Running; //safe to use even if it's paused, custom timescale, etc.
	}
}

public class FadeOut : GameCommand
{
	public override void OnEnter()
	{
		FadeHandler.FadeOut();
	}

	public override bool OnTick(float dt)
	{
		return !FadeHandler.Running; //safe to use even if it's paused, custom timescale, etc.
	}
}

public class PlayAudio : GameCommand
{
	[CommandParameter]
	private string soundID;

	[CommandParameter]
	private string soundMarkerPosition;

	[CommandParameter]
	private bool wait;

	[CommandParameter]
	private float radius = 40;

	private AudioSource sound;

	public override void OnEnter()
	{
		sound = AudioHandler.PlaySound(soundID, string.IsNullOrEmpty(soundMarkerPosition) ? null : gameplayState.CurrentWorld.FindMarkerPosition(soundMarkerPosition)?.Position, radius);
	}

	public override bool OnTick(float dt)
	{
		if (sound == null || !wait)
			return true;

		return !sound.IsPlaying;
	}
}

public class Spawn : GameCommand
{
	[CommandParameter]
	private string spawnMarkerPosition;
	[CommandParameter]
	private float cost = 1.0f;

	public override void OnEnter()
	{
		gameplayState.CurrentWorld.SpawnEntity<EnemyEntity>(nameof(EnemyEntity), e =>
		{
			e.Persistent = true;
			e.Position = gameplayState.CurrentWorld.FindMarkerPosition(spawnMarkerPosition).Position;
			e.Cost = MathF.Max(0.5f, cost);
		});
	}
}

public class Say : GameCommand
{
	[CommandParameter]
	private string character;

	[CommandParameter]
	private string message;

	[CommandParameter]
	private bool wait = true;

	private DialogueManager dm;
	private Dialogue dialogue;

	public override void OnEnter()
	{
		base.OnEnter();
		dm = gameplayState.GetManager<DialogueManager>();

		dialogue = new()
		{
			Character = character,
			Message = message
		};

		dm.ShowDialogue(dialogue);
	}

	public override bool OnTick(float dt)
	{
		return !wait || dm.CurrentDialogue == null || dm.CurrentDialogue != dialogue;
	}
}

public class SwitchLight : GameCommand
{
	[CommandParameter]
	private string id;

	[CommandParameter]
	private bool enabled;

	public override void OnEnter()
	{
		base.OnEnter();
		LightingSystem.SetLightGroupState(id, !enabled);
	}
}

public class Shake : GameCommand
{
	[CommandParameter]
	private float amount = 0.8f;

	public override void OnEnter()
	{
		base.OnEnter();
		Game.Instance.Camera.Shake(amount, null);
	}
}

public class BeginRound : GameCommand
{
	public override void OnEnter()
	{
		base.OnEnter();
		gameplayState.GetManager<GameplayManager>().Begin();
	}
}

public class SetActive : GameCommand
{
	[CommandParameter]
	private string nameTag;

	[CommandParameter]
	private bool isActive = true;

	public override void OnEnter()
	{
		base.OnEnter();

		gameplayState.CurrentWorld.GetEntityByNameTag<BaseEntity>(nameTag).IsActive = isActive;
	}
}

public class SetTriggerActive : GameCommand
{
	[CommandParameter]
	private string id;

	[CommandParameter]
	private bool isActive = true;

	public override void OnEnter()
	{
		base.OnEnter();
		gameplayState.GetManager<TriggerManager>().Find(id).ForEach(p => p.Enabled = isActive);
	}
}