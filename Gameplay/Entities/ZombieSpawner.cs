using Main.Core;

namespace Main.Gameplay.Entities;

public class ZombieSpawner : BaseEntity
{
	[JsonProperty]
	public float FacingAngle { get; set; }

	public void Spawn()
	{
		gameplayState.CurrentWorld.SpawnEntity<ZombieEntity>(e => {
			e.Position = Position;
			e.FacingAngle = FacingAngle;
		});

		Log.Send($"Spawned zombie at: {Position}");
	}

	public override void Update(float dt, float udt)
	{
		base.Update(dt, udt);

		if (Raylib.IsKeyPressed(KeyboardKey.Q)) //super temporary example
			Spawn();
	}
}