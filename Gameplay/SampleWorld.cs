using Main.Gameplay;
using Main.Gameplay.Entities;

public static class SampleWorld
{
	public static World GetSampleWorldData()
	{
		//feel free to recommend sample data here
		var world = new World();

		world.Entities.Add(new WallEntity()
		{
			Position = new(4, 0),
			Size = new(2, 5)
		});

		return world;
	}
}