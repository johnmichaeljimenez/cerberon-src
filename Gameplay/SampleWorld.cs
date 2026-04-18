using Main.Gameplay;
using Main.Gameplay.Entities;

public static class SampleWorld
{
	public static World GetSampleWorldData()
	{
		//feel free to recommend sample data here
		var world = new World()
		{
			WorldSettings = new()
			{
				PlayerSpawnPoint = new(10, -15)
			}	
		};

		world.Entities.Add(new WallEntity()
		{
			Position = new(4, 0),
			Size = new(2, 5)
		});

		world.Entities.Add(new WallEntity()
		{
			Position = new(-3, -4),
			Size = new(15, 6)
		});

		world.Entities.Add(new ZombieEntity()
		{
			Position = new(-10, 10),
			FacingAngle = 90
		});



		var n = 1;
		foreach (var i in world.Entities)
		{
			i.ID = n++;
		}

		return world;
	}
}