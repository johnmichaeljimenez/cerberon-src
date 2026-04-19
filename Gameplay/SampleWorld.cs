using Main.Core;
using Main.Effects;
using Main.Gameplay;
using Main.Gameplay.Entities;

public static class SampleWorld
{
    public static World GetSampleWorldData() //i am too lazy to make a JSON level right now, I'll go back on it once I have a proper minimum game loop
    {
        var world = new World()
        {
            WorldSettings = new()
            {
                PlayerSpawnPoint = new(10, -15), // +Y = downward
                WorldSize = new(120, 120),
                AmbientColor = new(40, 40, 80)
            }
        };

        //wall position is the top-left of wall
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

        world.Entities.Add(new ZombieSpawner()
        {
            Position = new(15, -20),
            FacingAngle = 90
        });

        LightingSystem.AddLight(AssetManager.GetSprite("light"), new(-20, 10), Colors.RED, 0, 30); //sprite, position, color, rotation, scale

        var n = 1;
        foreach (var i in world.Entities)
        {
            i.ID = n++;
        }

        return world;
    }
}