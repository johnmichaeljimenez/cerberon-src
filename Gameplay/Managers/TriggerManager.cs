using Main.Core;
using Main.Gameplay.Entities;
using Main.Gameplay.Entities.Player;
using Main.Helpers;

namespace Main.Gameplay.Managers;

public enum TriggerSurfaceType
{
	Default,
	Wood,
	Metal,
	Grass,
	Dirt,
	Gravel
}

public class Trigger
{
	[JsonProperty]
	public Vector2 Position;
	[JsonProperty]
	public Vector2 Size;
	[JsonProperty]
	public TriggerSurfaceType? TriggerSurfaceType { get; set; } //null means no footstep override (keep anything current)
	[JsonProperty]
	public string TriggerID { get; set; }
	[JsonProperty]
	public string AmbientAudio { get; set; }    //null means no ambient (keep anything current), empty means silence
	[JsonProperty]
	public int SortingIndex { get; set; }
	[JsonProperty]
	public float Rotation { get; set; }
	[JsonProperty]
	public bool Enabled { get; set; } = true;

	[JsonIgnore]
	public bool IsTriggered { get; set; }
}

public class TriggerManager : BaseManager
{
	//triggers have 2 purpose:
	//1 - execute something as one-shot
	//2 - define current region stepped on by player (used for ambient, footstep material, etc)

	private readonly List<Trigger> triggers = new();
	private readonly HashSet<Trigger> activeTriggers = new();

	public TriggerSurfaceType CurrentSurfaceType { get; private set; } = TriggerSurfaceType.Default;
	public string CurrentAmbientAudio { get; private set; } = "";

	public readonly Signal<(CharacterEntity, Trigger)> OnTriggerEnter = new();
	public readonly Signal<(CharacterEntity, Trigger)> OnTriggerExit = new();
	public readonly Signal<(CharacterEntity, Trigger)> OnTriggerExecute = new();

	public TriggerManager(GameplayState gameplayState) : base(gameplayState)
	{

	}

	public void SetupTriggers(IEnumerable<Trigger> triggers)
	{
		this.triggers.Clear();

		if (triggers != null)
			this.triggers.AddRange(triggers.OrderBy(p => p.SortingIndex).ThenBy(p => p.TriggerID));

		foreach (var t in this.triggers)
			t.IsTriggered = false;

		RefreshCurrentEnvironment();
	}

	public override void Update(float dt, float udt)
	{
		base.Update(dt, udt);

		var pc = gameplayState.GetManager<GameplayManager>().PlayerCharacter;

		if (pc.IsDead)
			return;

		foreach (var t in triggers)
		{
			if (!t.Enabled)
				continue;

			bool isInside = Utils.CheckCollisionCircleRec(pc.Position, pc.Radius, t.Position, t.Size, t.Rotation);

			if (isInside)
			{
				if (activeTriggers.Add(t))
					OnTriggerEnterEvent(pc, t);

				if (!t.IsTriggered)
					OnTriggerExecuteEvent(pc, t);
			}
			else
			{
				if (activeTriggers.Remove(t))
					OnTriggerExitEvent(pc, t);
			}
		}
	}

	private void OnTriggerEnterEvent(PlayerEntity c, Trigger t)
	{
		OnTriggerEnter.Publish((c, t));
		RefreshCurrentEnvironment();
	}

	private void OnTriggerExitEvent(PlayerEntity c, Trigger t)
	{
		OnTriggerExit.Publish((c, t));
		RefreshCurrentEnvironment();
	}

	private void OnTriggerExecuteEvent(PlayerEntity c, Trigger t)
	{
		foreach (var i in triggers)
		{
			if (i == t || i.TriggerID == t.TriggerID)
				i.IsTriggered = true; //no need for multi-shot triggers right now, and disable all triggers with same group on activate
		}

		Log.Send("Trigger execute by player");
		OnTriggerExecute.Publish((c, t));
	}

	public override void DrawDebug()
	{
		base.DrawDebug();

		foreach (var i in triggers)
		{
			var rec = new Rectangle(i.Position, i.Size);
			Raylib.DrawRectanglePro(rec, rec.Size * 0.5f, i.Rotation, Color.Green);
		}
	}

	private void RefreshCurrentEnvironment()
	{
		CurrentSurfaceType = TriggerSurfaceType.Default;
		CurrentAmbientAudio = gameplayState.CurrentWorld.WorldSettings.AmbientSound ?? "";
		AudioHandler.SetAmbient(CurrentAmbientAudio);

		if (activeTriggers.Count == 0)
			return;

		for (int i = triggers.Count - 1; i >= 0; i--)
		{
			var t = triggers[i];
			if (activeTriggers.Contains(t))
			{
				if (t.TriggerSurfaceType.HasValue)
					CurrentSurfaceType = t.TriggerSurfaceType.Value;

				if (t.AmbientAudio != null)
				{
					CurrentAmbientAudio = t.AmbientAudio;
					AudioHandler.SetAmbient(CurrentAmbientAudio);
				}

				return;
			}
		}
	}

	public List<Trigger> Find(string id)
	{
		return triggers.Where(p => p.TriggerID == id).ToList();
	}
}