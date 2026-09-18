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
	private Dictionary<string, Dictionary<string, Instruction>> groupedScripts = new();

	public GameplayEventManager(GameplayState gameplayState) : base(gameplayState)
	{
		engine = new(gameplayState);
		CompileScripts(new() { "Main" });
	}

	public void CompileScripts(List<string> directories)
	{
		var path = "Assets/Scripts";
		foreach (var file in Directory.GetFiles(path, "*.ops", SearchOption.AllDirectories))
		{
			var relativePath = Path.GetRelativePath(path, file);
			var pathParts = relativePath.Split(Path.DirectorySeparatorChar);
			if (pathParts.Length == 0) continue;

			var baseDirectory = pathParts[0];
			var instruction = engine.CompileFile(file);

			instruction.ID = $"{baseDirectory}/{instruction.ID}";

			if (!groupedScripts.ContainsKey(baseDirectory))
			{
				groupedScripts[baseDirectory] = new Dictionary<string, Instruction>();
			}

			groupedScripts[baseDirectory].Add(instruction.ID, instruction);
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