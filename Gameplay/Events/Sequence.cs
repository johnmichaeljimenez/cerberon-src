namespace Cerberon.Gameplay.Events;

public class Sequence
{
	private readonly Queue<BaseCommand> commands = new();
	public string ID { get; private set; }

	protected readonly GameplayState gameplayState;
	public bool IsRunning { get; private set; }

	public Sequence(GameplayState gameplayState, string id, params BaseCommand[] commands)
	{
		this.gameplayState = gameplayState;
		ID = id;

		foreach (var i in commands)
		{
			this.commands.Enqueue(i);
			i.Setup(gameplayState);
		}

		IsRunning = true;
		this.commands.Peek().OnEnter();
	}

	public void Update(float dt)
	{
		if (!IsRunning || commands.Count == 0)
		{
			IsRunning = false;
			return;
		}

		var c = commands.Peek();
		if (c.Update(dt))
		{
			c.OnExit();
			commands.Dequeue();

			if (commands.Count > 0)
				commands.Peek().OnEnter();
			else
				IsRunning = false;
		}
	}
}