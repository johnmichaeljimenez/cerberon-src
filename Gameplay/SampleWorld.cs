using Main.Core;
using Main.Effects;
using Main.Gameplay;
using Main.Gameplay.Entities;

public static class SampleWorld
{
    public static World GetSampleWorldData()
    {
        var world = new World()
        {
            WorldSettings = new()
            {
                PlayerSpawnPoint = new(10, -15) // +Y = downward
            }	
        };

        // ===================================================================
        // WALLS SETUP - CLEANED + HEAVILY EXTENDED
        // ===================================================================
        // • Original walls kept for backward compatibility
        // • Added outer boundaries, multiple rooms, obstacles, and corridors
        // • All hallways/corridors are >= 3 units wide (player size = 1 → safe margin)
        // • Walls are defined in a list for easy editing/expansion later
        var walls = new List<WallEntity>
        {
            // Original walls
            new WallEntity
            {
                Position = new(4, 0),
                Size = new(2, 5)
            },
            new WallEntity
            {
                Position = new(-3, -4),
                Size = new(15, 6)
            },

            // Outer boundaries (big playable area ~ -25..35 x, -30..20 y)
            new WallEntity { Position = new(-25, -30), Size = new(3, 55) },   // left vertical wall
            new WallEntity { Position = new(35, -30), Size = new(3, 55) },   // right vertical wall
            new WallEntity { Position = new(-22, -30), Size = new(59, 3) },  // top horizontal wall

            // Bottom "floor" with intentional gaps for open feel / future exits
            new WallEntity { Position = new(-25, 15), Size = new(22, 4) },   // bottom-left floor
            new WallEntity { Position = new(12, 15), Size = new(26, 4) },    // bottom-right floor (gap in middle)

            // Inner room dividers & corridors (all gaps ≥ 3 units)
            new WallEntity { Position = new(5, -22), Size = new(2, 15) },    // vertical room divider (creates left/right areas)
            new WallEntity { Position = new(-20, -12), Size = new(18, 2) },  // horizontal inner wall (upper room ceiling)
            new WallEntity { Position = new(20, -18), Size = new(2, 20) },   // tall vertical wall for right-side rooms

            // Small obstacles / pillars for cover & visual interest
            new WallEntity { Position = new(15, -16), Size = new(4, 4) },    // square pillar near spawn area
            new WallEntity { Position = new(-12, -2), Size = new(5, 3) },    // low obstacle on main floor
            new WallEntity { Position = new(28, 2), Size = new(3, 10) },     // tall pillar on right side

            // Example corridor (parallel walls with 3-unit gap)
            new WallEntity { Position = new(-18, 4), Size = new(2, 12) },    // corridor left wall
            new WallEntity { Position = new(-13, 4), Size = new(2, 12) },    // corridor right wall → 3-unit hallway

            // Extra walls for more maze-like depth
            new WallEntity { Position = new(0, 8), Size = new(10, 2) },      // short horizontal floor segment
            new WallEntity { Position = new(-8, -27), Size = new(2, 6) },    // short vertical stub near top-left
            new WallEntity { Position = new(30, -25), Size = new(2, 9) },    // vertical stub on far right
        };

        // Add all walls to the world
        foreach (var wall in walls)
        {
            world.Entities.Add(wall);
        }

        // ===================================================================
        // LIGHTS SETUP - HEAVILY EXTENDED
        // ===================================================================
        // Multiple colored lights scattered across the new layout for nice atmosphere
        // Varied scales (intensity/size) and positions to cover rooms + corridors
        LightingSystem.AddLight(AssetManager.GetSprite("light"), new(-20, 10), Colors.RED,    0, 30); // original light (kept)
        LightingSystem.AddLight(AssetManager.GetSprite("light"), new(10, -20), Colors.WHITE,  0, 25); // near player spawn
        LightingSystem.AddLight(AssetManager.GetSprite("light"), new(30, 0),   Colors.BLUE,   0, 35); // right side room
        LightingSystem.AddLight(AssetManager.GetSprite("light"), new(-15, -15),Colors.GREEN,  0, 22); // upper-left area
        LightingSystem.AddLight(AssetManager.GetSprite("light"), new(25, -25), Colors.YELLOW, 0, 28); // top-right corner
        LightingSystem.AddLight(AssetManager.GetSprite("light"), new(5, 12),   Colors.RED,    0, 20); // near bottom floor
        LightingSystem.AddLight(AssetManager.GetSprite("light"), new(-22, 5),  Colors.BLUE,   0, 32); // corridor area
        LightingSystem.AddLight(AssetManager.GetSprite("light"), new(35, -12), Colors.WHITE,  0, 40); // far-right edge
        LightingSystem.AddLight(AssetManager.GetSprite("light"), new(15, 6),   Colors.GREEN,  0, 26); // central pillar area
        LightingSystem.AddLight(AssetManager.GetSprite("light"), new(-8, -8),  Colors.YELLOW, 0, 18); // extra accent in upper room

        // Assign sequential IDs to all entities (kept from original)
        var n = 1;
        foreach (var entity in world.Entities)
        {
            entity.ID = n++;
        }

        return world;
    }
}