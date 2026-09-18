using System.Text;
using Cerberon.Core;
using Cerberon.Gameplay.Events;
using OpcodeEngine.Core;

namespace Cerberon.Gameplay.Managers;

public enum EventTypes
{
	None,
	StartGame,
	Trigger
}

public class GameplayEventManager : BaseManager
{
	private readonly ScriptRunner engine;

	private readonly Dictionary<string, Instruction> scripts = new();

	public GameplayEventManager(GameplayState gameplayState) : base(gameplayState)
	{
		engine = new(gameplayState);

		var path = "Assets/Scripts";    //TODO: improve
		foreach (var i in Directory.GetFiles(path, "*.ops", SearchOption.AllDirectories))
		{
			var instruction = engine.CompileFile(i);
			scripts.Add(instruction.ID, instruction);
		}
	}

	public void FireTrigger(EventTypes eventType, params object[] args)
	{
		var trigger = $"{eventType} {string.Join(' ', args)}".Trim().ToUpper();
		engine.FireTrigger(trigger);
	}

	public override void Update(float dt, float udt)
	{
		base.Update(dt, udt);

		if (PauseHandler.IsPaused)
			return;

		engine.Tick(dt);
	}

	public override void DrawImGui()
	{
		base.DrawImGui();

		// if (ImGui.Button("Test"))
		// {
		// 	RunEvent("power",
		// 		new PlayAudio("phone", null, true),
		// 		new Wait(0.1f),
		// 		new PlayAudio("phone", null, true),
		// 		new ShowDialogue("power-1", false),
		// 		new Wait(0.5f)
		// 	);
		// }
	}
}

public class ScriptRunner : Engine
{
	public GameplayState gameplayState;

	public ScriptRunner(GameplayState gameplayState)
	{
		this.gameplayState = gameplayState;
	}
}