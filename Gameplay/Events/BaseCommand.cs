using Main.Core;
using Main.Gameplay.Entities;

namespace Main.Gameplay.Events;

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

	public PlayAudio(string id, Vector2 pos, bool wait = false)
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
	public Vector2 position;

	public SpawnEnemy(Vector2 pos)
	{
		position = pos;
	}

	public override void OnEnter()
	{
		gameplayState.CurrentWorld.SpawnEntity<EnemyEntity>(nameof(EnemyEntity), e =>
		{
			e.Persistent = true;
			e.Position = position;
		});
	}
}