// Based on Sebastian Lague's implementation
// Original: https://github.com/SebLague/Poisson-Disc-Sampling

namespace Main.Helpers
{
    public static class PoissonDisc
    {
        public static List<Vector2> Sample(Rectangle rect, float radius, int k = 30)
        {
            Vector2 regionSize = new Vector2(rect.Width, rect.Height);
            float cellSize = radius / MathF.Sqrt(2f);

            int gridWidth = (int)Math.Ceiling(regionSize.X / cellSize);
            int gridHeight = (int)Math.Ceiling(regionSize.Y / cellSize);

            int[,] grid = new int[gridWidth, gridHeight];
            List<Vector2> points = new List<Vector2>();
            List<Vector2> active = new List<Vector2>();
            Random rng = new Random();  //TODO: make a proper Random static class

            Vector2 first = regionSize * 0.5f;
            points.Add(first);
            active.Add(first);
            grid[(int)(first.X / cellSize), (int)(first.Y / cellSize)] = points.Count;

            while (active.Count > 0)
            {
                int index = rng.Next(active.Count);
                Vector2 center = active[index];
                bool found = false;

                for (int i = 0; i < k; i++)
                {
                    float angle = (float)(rng.NextDouble() * MathF.Tau);
                    Vector2 dir = new Vector2(MathF.Sin(angle), MathF.Cos(angle));
                    float dist = radius * (1f + (float)rng.NextDouble());
                    Vector2 candidate = center + dir * dist;

                    if (IsValid(candidate, regionSize, cellSize, radius, points, grid))
                    {
                        points.Add(candidate);
                        active.Add(candidate);
                        grid[(int)(candidate.X / cellSize), (int)(candidate.Y / cellSize)] = points.Count;
                        found = true;
                        break;
                    }
                }

                if (!found)
                    active.RemoveAt(index);
            }

            for (int i = 0; i < points.Count; i++)
                points[i] += new Vector2(rect.X, rect.Y);

            return points;
        }

        private static bool IsValid(
            Vector2 candidate,
            Vector2 regionSize,
            float cellSize,
            float radius,
            List<Vector2> points,
            int[,] grid)
        {
            if (candidate.X < 0 || candidate.X >= regionSize.X ||
                candidate.Y < 0 || candidate.Y >= regionSize.Y)
                return false;

            int cellX = (int)(candidate.X / cellSize);
            int cellY = (int)(candidate.Y / cellSize);

            int startX = Math.Max(0, cellX - 2);
            int endX = Math.Min(cellX + 2, grid.GetLength(0) - 1);
            int startY = Math.Max(0, cellY - 2);
            int endY = Math.Min(cellY + 2, grid.GetLength(1) - 1);

            float radiusSq = radius * radius;

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    int idx = grid[x, y] - 1;
                    if (idx != -1 && Vector2.DistanceSquared(candidate, points[idx]) < radiusSq)
                        return false;
                }
            }
            
            return true;
        }
    }
}