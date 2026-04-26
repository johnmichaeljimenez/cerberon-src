using Main.Core;
using Main.Effects;
using Main.Gameplay;
using Main.Gameplay.Entities;
using Main.Helpers;

public static class SampleWorld
{
    public static World GetSampleWorldData()
    {
        var lightSprite = AssetManager.GetSprite("light");
        var world = new World()
        {
            WorldSettings = new()
            {
                PlayerSpawnPoint = new(25, 31),
                WorldSize = new(160, 160),
                AmbientColor = new(10, 10, 20)
            }
        };

        world.EnvironmentSprites.Add(new()
        {
            Position = new(25, 30),
            Scale = 1,
            Parallax = 0,
            Rotation = 0,
            SortingGroup = 0,
            SortingIndex = 0,
            SpriteID = "env/tile-2",
            Tint = Color.White,
            RenderType = WorldSpriteRenderer.RenderTypes.Tiled,
            TileSize = new Vector2(25, 25)
        });

        world.EnvironmentSprites.Add(new()
        {
            Position = new(25, 30),
            Scale = 2,
            Parallax = 0.2f,
            Rotation = 0,
            SortingGroup = 1,
            SortingIndex = 0,
            SpriteID = "env/overlay-bush-01",
            Tint = Color.Black
        });

        world.Entities.Add(new WallEntity { Position = new(-66.0f, -48.0f), Size = new(6.0f, 96.0f) });
        world.Entities.Add(new WallEntity { Position = new(69.0f, -78.0f), Size = new(6.0f, 126.0f) });
        world.Entities.Add(new WallEntity { Position = new(0.0f, -78.0f), Size = new(69.0f, 6.0f) });
        world.Entities.Add(new WallEntity { Position = new(-66.0f, 45.0f), Size = new(36.0f, 6.0f) });
        world.Entities.Add(new WallEntity { Position = new(0.0f, 45.0f), Size = new(75.0f, 6.0f) });
        world.Entities.Add(new WallEntity { Position = new(-30.0f, 63.0f), Size = new(9.0f, 6.0f) });
        world.Entities.Add(new WallEntity { Position = new(-60.0f, -48.0f), Size = new(30.0f, 6.0f) });
        world.Entities.Add(new WallEntity { Position = new(-30.0f, -24.0f), Size = new(30.0f, 6.0f) });
        world.Entities.Add(new WallEntity { Position = new(-36.0f, -48.0f), Size = new(6.0f, 15.0f) });
        world.Entities.Add(new WallEntity { Position = new(0.0f, -27.0f), Size = new(6.0f, 9.0f) });
        world.Entities.Add(new WallEntity { Position = new(-36.0f, 48.0f), Size = new(6.0f, 21.0f) });
        world.Entities.Add(new WallEntity { Position = new(0.0f, 48.0f), Size = new(6.0f, 21.0f) });
        world.Entities.Add(new WallEntity { Position = new(0.0f, -12.0f), Size = new(6.0f, 24.0f) });
        world.Entities.Add(new WallEntity { Position = new(-36.0f, 12.0f), Size = new(6.0f, 18.0f) });
        world.Entities.Add(new WallEntity { Position = new(45.0f, -12.0f), Size = new(6.0f, 24.0f) });
        world.Entities.Add(new WallEntity { Position = new(6.0f, -24.0f), Size = new(45.0f, 6.0f) });
        world.Entities.Add(new WallEntity { Position = new(0.0f, 21.0f), Size = new(6.0f, 9.0f) });
        world.Entities.Add(new WallEntity { Position = new(6.0f, 6.0f), Size = new(12.0f, 6.0f) });
        world.Entities.Add(new WallEntity { Position = new(6.0f, 21.0f), Size = new(12.0f, 6.0f) });
        world.Entities.Add(new WallEntity { Position = new(24.0f, 21.0f), Size = new(21.0f, 6.0f) });
        world.Entities.Add(new WallEntity { Position = new(24.0f, 6.0f), Size = new(21.0f, 6.0f) });
        world.Entities.Add(new WallEntity { Position = new(45.0f, 21.0f), Size = new(6.0f, 15.0f) });
        world.Entities.Add(new WallEntity { Position = new(-36.0f, 36.0f), Size = new(6.0f, 9.0f) });
        world.Entities.Add(new WallEntity { Position = new(-36.0f, -27.0f), Size = new(6.0f, 33.0f) });
        world.Entities.Add(new WallEntity { Position = new(-9.0f, 63.0f), Size = new(9.0f, 6.0f) });
        world.Entities.Add(new WallEntity { Position = new(-60.0f, 21.0f), Size = new(15.0f, 6.0f) });
        world.Entities.Add(new WallEntity { Position = new(-18.0f, -6.0f), Size = new(6.0f, 6.0f) });
        world.Entities.Add(new WallEntity { Position = new(-18.0f, 39.0f), Size = new(6.0f, 6.0f) });
        world.Entities.Add(new WallEntity { Position = new(0.0f, 36.0f), Size = new(6.0f, 9.0f) });
        world.Entities.Add(new WallEntity { Position = new(-39.0f, 21.0f), Size = new(3.0f, 6.0f) });
        world.Entities.Add(new WallEntity { Position = new(0.0f, -72.0f), Size = new(6.0f, 39.0f) });
        world.Entities.Add(new WallEntity { Position = new(-30.0f, -42.0f), Size = new(30.0f, 6.0f) });
        world.Entities.Add(new WallEntity { Position = new(-60.0f, -15.0f), Size = new(15.0f, 6.0f) });
        world.Entities.Add(new WallEntity { Position = new(-39.0f, -15.0f), Size = new(3.0f, 6.0f) });
        world.Entities.Add(new WallEntity { Position = new(45.0f, 42.0f), Size = new(6.0f, 3.0f) });

        LightingSystem.AddLight(lightSprite, new(-14.5f, 22), Colors.WHITE.Fade(0.8f), 0, 64, shadowType: Light.ShadowTypes.Static);
        LightingSystem.AddLight(lightSprite, new(22, 36), Colors.WHITE.Fade(0.8f), 0, 64, shadowType: Light.ShadowTypes.Static);
        LightingSystem.AddLight(lightSprite, new(-38, 33), Colors.WHITE.Fade(0.8f), 0, 64, shadowType: Light.ShadowTypes.Static);

        var n = 1;
        foreach (var i in world.Entities)
        {
            i.ID = n++;
        }

        return world;
    }
}