using Main.Helpers;

namespace Main.Gameplay.Managers;

//TODO: make all magic numbers editable outside
public class AIDirectorManager : BaseManager
{
	public enum TensionState { Calm, Tense, Panic, Critical }

	public TensionState CurrentState =>
		Tension switch
		{
			< 0.25f => TensionState.Calm,
			< 0.5f => TensionState.Tense,
			< 0.9f => TensionState.Panic,
			_ => TensionState.Critical
		};

	private readonly EMA emaPlayerHealth = new(0.01f);
	private readonly EMA emaPlayerAmmo = new(0.05f);
	private readonly EMA emaTension = new(0.08f);

	private float baseTension; //rigid and only going forward, no need for EMA
	private float playerStrength; //EMA provided by emaTension

	public float Tension { get; private set; } //controls enemy aggressiveness, spawn rate, and mood
	public float DifficultyModifier { get; private set; } //controls drop rates (other than that, not sure yet tbh)

	public AIDirectorManager(GameplayState gameplayState) : base(gameplayState)
	{

	}

	public override void Update(float dt, float udt)
	{
		base.Update(dt, udt);

		var gm = gameplayState.GetManager<GameplayManager>();
		if (!gm.Running)
			return;

		var pc = gameplayState.GetManager<PlayerManager>().PlayerCharacter;

		emaPlayerHealth.AddSample((float)pc.HP / (float)pc.MaxHP);
		//for quick dev, guns and max ammos are guaranteed as non-zero
		emaPlayerAmmo.AddSample(pc.guns.Sum(p => (float)(p.CurrentAmmo + p.CurrentMaxAmmo) / (float)(p.MagSize + p.MaxAmmo)) / (float)pc.guns.Count); //TODO: add Signal<Gun> on gun fire for ammo change instead of continuous per-frame query

		var healthFactor = emaPlayerHealth.Current;
		var ammoFactor = emaPlayerAmmo.Current;

		playerStrength = healthFactor * 0.95f + ammoFactor * 0.05f;
		baseTension = GetBaseTensionRamp(1f - (gm.GameTime / gm.MaxGameTime)); //GameTime counts down, 0 = win ending, and the lower the number, the more intense the gameplay (zombie home invasion from midnight until dawn)

		var deviation = playerStrength - 0.5f;
		var newTension = baseTension + deviation; //player's status "rides" the baseTension without straying too far

		DifficultyModifier = playerStrength;

		emaTension.AddSample(newTension);
		Tension = Math.Clamp(emaTension.Current, 0f, 1f);
	}

	private float GetBaseTensionRamp(float t)
	{
		//ease in out circ
		//slow ramp up at start, sudden spike at middle, then slow towards peak until the end
		return t < 0.5f ? (1 - MathF.Sqrt(1 - MathF.Pow(2 * t, 2))) / 2
  						: (MathF.Sqrt(1 - MathF.Pow(-2 * t + 2, 2)) + 1) / 2;
	}

	public override void DrawImGui()
	{
		base.DrawImGui();

		ImGui.Text($"Tension: {Tension:F2} ({CurrentState})");
		ImGui.Text($"Base Tension: {baseTension:F2}");
		ImGui.Text($"Difficulty Modifier: {DifficultyModifier:F2}");
		ImGui.Text($"Player Strength: {playerStrength:F2}");
		ImGui.Text($"Player Health: {emaPlayerHealth.Current:F2}");
		ImGui.Text($"Player Ammo: {emaPlayerAmmo.Current:F2}");
	}
}
