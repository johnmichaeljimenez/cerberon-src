using System.Text;
using Cerberon.Core;
using Cerberon.Gameplay.Entities;
using Cerberon.Gameplay.Entities.Player;
using Cerberon.Gameplay.Events;
using Cerberon.Gameplay.Level;
using Cerberon.Helpers;
using OpcodeEngine.Core;

namespace Cerberon.Gameplay.Managers;

public enum EventTypes
{
	None,
	StartGame,
	TimeEnd,
	Trigger,
	Interact
}

public class GameplayEventManager : BaseManager
{
	private readonly ScriptRunner engine;
	private Dictionary<string, Dictionary<string, Instruction>> scripts = new();

	public GameplayEventManager(GameplayState gameplayState) : base(gameplayState)
	{
		engine = new(gameplayState);

		gameplayState.GetManager<GameplayManager>().OnGameStart.Subscribe(_ =>
		{
			FireTrigger(EventTypes.StartGame);
		}).AddTo(disposables);

		gameplayState.GetManager<GameplayManager>().OnTimeEnd.Subscribe(_ =>
		{
			FireTrigger(EventTypes.TimeEnd);
		}).AddTo(disposables);
	}

	public override void Init()
	{
		base.Init();

		gameplayState.GetManager<TriggerManager>().OnTriggerExecute.Subscribe(t =>
		{
			FireTrigger(EventTypes.Trigger, t.Item2.TriggerID);
		}).AddTo(disposables);
	}

	public void CompileScripts(List<string> directories)
	{
		if (directories.Count == 0)
			return;

		var basePath = "Assets/Scripts";

		if (!Directory.Exists(basePath)) return;

		foreach (var selectedDir in directories)
		{
			var targetFolderPath = Path.Combine(basePath, selectedDir);

			if (!Directory.Exists(targetFolderPath))
				continue;

			foreach (var file in Directory.GetFiles(targetFolderPath, "*.ops", SearchOption.AllDirectories))
			{
				var relativePath = Path.GetRelativePath(basePath, file);

				var pathParts = relativePath.Split(Path.DirectorySeparatorChar);
				if (pathParts.Length == 0)
					continue;

				var baseDirectory = pathParts[0];

				var instruction = engine.CompileFile(file);
				instruction.ID = $"{baseDirectory}/{instruction.ID}";

				if (!scripts.ContainsKey(baseDirectory))
				{
					scripts[baseDirectory] = new Dictionary<string, Instruction>();
				}

				scripts[baseDirectory][instruction.ID] = instruction;
				instructions.Add(instruction);
			}
		}

		instructionNames = instructions.Select(p => string.IsNullOrEmpty(p.TriggerKey)? p.ID : $"{p.ID} (@{p.TriggerKey})").Prepend("<Custom>").ToArray();
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

	private int dropdownIndex;
	private readonly List<Instruction> instructions = new();
	private string[] instructionNames;
	public override void DrawImGui()
	{
		base.DrawImGui();

		if (ImGui.Combo("Scripts", ref dropdownIndex, instructionNames, instructionNames.Length))
		{
			
		}

		if (dropdownIndex > 0)
		{
			if (ImGui.Button("Trigger"))
				engine.Run(instructions[dropdownIndex-1]);
		}

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