using Main.Core;
using Main.Effects;
using Main.Gameplay;
using Main.Gameplay.Entities;
using Main.Helpers;

public static class SampleWorld
{
    public static World GetSampleWorldData()
    {
        var world = new World()
        {
            WorldSettings = new()
            {
                PlayerSpawnPoint = new(10, -15),   // +Y = downward
                WorldSize = new(120, 120),
                AmbientColor = new(40, 40, 80)
            }
        };

        // ---------- Warehouse outer walls ----------
        // Bottom wall
        world.Entities.Add(new WallEntity()
        {
            Position = new(-60, 60),   // top‑left corner
            Size = new(120, 2)         // width, height
        });
        // Top wall
        world.Entities.Add(new WallEntity()
        {
            Position = new(-60, -62),
            Size = new(120, 2)
        });
        // Left wall
        world.Entities.Add(new WallEntity()
        {
            Position = new(-62, -60),
            Size = new(2, 120)
        });
        // Right wall
        world.Entities.Add(new WallEntity()
        {
            Position = new(60, -60),
            Size = new(2, 120)
        });

        // ---------- Interior “shelf” walls ----------
        // Central aisle (vertical)
        world.Entities.Add(new WallEntity()
        {
            Position = new(0, -50),
            Size = new(2, 100)
        });

        // Two horizontal cross‑beams (like loading docks)
        world.Entities.Add(new WallEntity()
        {
            Position = new(-40, -20),
            Size = new(80, 2)
        });
        world.Entities.Add(new WallEntity()
        {
            Position = new(-40, 20),
            Size = new(80, 2)
        });

        // ---------- Zombie spawner (keep original) ----------
        world.Entities.Add(new ZombieSpawner()
        {
            Position = new(15, -20),
            FacingAngle = 90
        });

        // ---------- Warehouse lighting ----------
        // Ceiling lights spaced every ~30 units along the top row
        var lightSprite = AssetManager.GetSprite("light");
        var lightColor = Colors.WHITE;   // neutral warehouse lighting

        // left‑side ceiling light
        LightingSystem.AddLight(lightSprite, new(-45, -55), lightColor, 0, 30);
        // centre‑ceiling light
        LightingSystem.AddLight(lightSprite, new(0, -55), lightColor, 0, 30);
        // right‑side ceiling light
        LightingSystem.AddLight(lightSprite, new(45, -55), lightColor, 0, 30);

        // Optional floor‑level safety light near the entrance
        LightingSystem.AddLight(lightSprite, new(10, -10), Colors.YELLOW, 0, 20);

        // ---------- Assign IDs ----------
        var n = 1;
        foreach (var i in world.Entities)
        {
            i.ID = n++;
        }

        return world;
    }
}