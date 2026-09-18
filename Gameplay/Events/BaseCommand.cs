using Cerberon.Core;
using Cerberon.Effects;
using Cerberon.Gameplay.Entities;
using Cerberon.Gameplay.Managers;
using OpcodeEngine.Commands;

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
	private AudioSource sound;
	private string soundID;
	private Vector2? soundPosition;
	private bool wait;
	private float radius;

	public PlayAudio(string id, Vector2? pos, bool wait = false, float radius = 40)
	{
		soundID = id;
		soundPosition = pos;
		this.wait = wait;
		this.radius = radius;
	}

	public override void OnEnter()
	{
		sound = AudioHandler.PlaySound(soundID, soundPosition, radius);
	}

	public override bool OnTick(float dt)
	{
		if (sound == null || !wait)
			return true;

		return !sound.IsPlaying;
	}
}

public class SpawnEnemy : GameCommand
{
	private Vector2 position;
	private float cost;

	public SpawnEnemy(Vector2 pos, float cost = 1.0f)
	{
		position = pos;
		this.cost = cost;
	}

	public override void OnEnter()
	{
		gameplayState.CurrentWorld.SpawnEntity<EnemyEntity>(nameof(EnemyEntity), e =>
		{
			e.Persistent = true;
			e.Position = position;
			e.Cost = MathF.Max(0.5f, cost);
		});
	}
}

public class Say : GameCommand
{
	private DialogueManager dm;
	private bool wait = true;
	private Dialogue dialogue;

	public override void OnInit(params string[] args)
	{
		dialogue = new()
		{
			Character = args[0],
			Message = args[1]
		};
	}

	public override void OnEnter()
	{
		base.OnEnter();
		dm = gameplayState.GetManager<DialogueManager>();
		dm.ShowDialogue(dialogue);
	}

	public override bool OnTick(float dt)
	{
		return !wait || dm.CurrentDialogue == null || dm.CurrentDialogue != dialogue;
	}
}

public class SetLightGroupState : GameCommand
{
	private string id;
	private bool enabled;

	public SetLightGroupState(string id, bool enabled)
	{
		this.id = id;
		this.enabled = enabled;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		LightingSystem.SetLightGroupState(id, !enabled);
	}
}