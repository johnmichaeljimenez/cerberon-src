using Cerberon.Core;
using Cerberon.Effects;
using Cerberon.Gameplay.Entities;
using Cerberon.Gameplay.Managers;

namespace Cerberon.Gameplay.Events;

public abstract class BaseCommand
{
	protected GameplayState gameplayState;

	public virtual void Setup(GameplayState gameplayState) //use this so that constructors are tiny for each command
	{
		this.gameplayState = gameplayState;
	}

	//everything below can be safely fully overriden
	public virtual void OnEnter()
	{

	}

	public virtual bool Update(float dt)
	{
		return true;
	}

	public virtual void OnExit()
	{

	}
}

public class Wait : BaseCommand
{
	private float t;

	public Wait(float duration)
	{
		t = duration;
	}

	public override bool Update(float dt)
	{
		t -= dt;
		return t <= 0;
	}
}

public class Print : BaseCommand
{
	private string msg;

	public Print(string msg)
	{
		this.msg = msg;
	}

	public override void OnEnter()
	{
		Log.Send(msg);
	}
}

public class Fade : BaseCommand
{
	private bool fadeIn;

	public Fade(bool fadeIn)
	{
		this.fadeIn = fadeIn;
	}

	public override void OnEnter()
	{
		if (fadeIn)
			FadeHandler.FadeIn();
		else
			FadeHandler.FadeOut();
	}

	public override bool Update(float dt)
	{
		return !FadeHandler.Running; //safe to use even if it's paused, custom timescale, etc.
	}
}

public class PlayAudio : BaseCommand
{
	private Sound? sound;
	private string soundID;
	private Vector2? soundPosition;
	private bool wait;

	public PlayAudio(string id, Vector2? pos, bool wait = false)
	{
		soundID = id;
		soundPosition = pos;
		this.wait = wait;
	}

	public override void OnEnter()
	{
		sound = AudioHandler.PlaySound(soundID, soundPosition);
	}

	public override bool Update(float dt)
	{
		if (!sound.HasValue || !wait)
			return true;

		return AudioHandler.IsPlaying(sound.Value);
	}
}

public class SpawnEnemy : BaseCommand
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

public class Exec : BaseCommand
{
	private Action onAction;

	public Exec(Action onAction)
	{
		this.onAction = onAction;
	}

	public override void OnEnter()
	{
		onAction?.Invoke();
	}

	public override bool Update(float dt)
	{
		return true;
	}
}

public class ShowDialogue : BaseCommand
{
	private string id;
	private DialogueManager dm;
	private bool wait;

	public ShowDialogue(string id, bool wait)
	{
		this.id = id;
		this.wait = wait;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		dm = gameplayState.GetManager<DialogueManager>();
		dm.ShowDialogue(id);
	}

	public override bool Update(float dt)
	{
		return !wait || dm.CurrentDialogue == null || dm.CurrentDialogue.ID != id;
	}
}

public class SetLightGroupState : BaseCommand
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