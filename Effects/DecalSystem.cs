using Main.Core;
using Main.Helpers;

namespace Main.Effects;

public static class DecalSystem
{
	public class DecalRegion : IDisposable
	{
		public Rectangle WorldRect { get; private set; }

		public RenderTexture2D RenderTexture { get; private set; }

		public DecalRegion(Rectangle worldRect, int textureSize)
		{
			WorldRect = worldRect;

			RenderTexture = Raylib.LoadRenderTexture(textureSize, textureSize);
			Raylib.SetTextureFilter(RenderTexture.Texture, TextureFilter.Point); // avoid seams

			Raylib.BeginTextureMode(RenderTexture);
			Raylib.ClearBackground(Color.Blank);
			Raylib.EndTextureMode();
		}

		public void Paint(Vector2 localDecalPos)
		{
			Raylib.BeginTextureMode(RenderTexture);

			//placeholder procedural blood for now
			var amt = RNG.Range(3, 12);
			for (int i = 0; i < amt; i++)
			{
				var pos = localDecalPos + RNG.Position(20);
				Raylib.DrawCircleV(pos, RNG.Range(1, 4), Colors.RED.Value(RNG.Range(0.4f, 0.6f))); //do not use any alpha < 1 here. blood doesn't become translucent on the floor on contact
			}

			Raylib.EndTextureMode();
		}

		public void Dispose()
		{
			Raylib.UnloadRenderTexture(RenderTexture);
		}
	}

	private static readonly List<DecalRegion> regions = new();

	private const float UNIT_SIZE_WORLD = 32f;        //world units per chunk
	private const int DECAL_TEXTURE_SIZE = 512;          // lower = less HD
	private const float DECAL_PPU = DECAL_TEXTURE_SIZE / UNIT_SIZE_WORLD;

	public static void Init(Vector2 worldOrigin, Vector2 worldSize)
	{
		var worldRect = new Rectangle(worldOrigin - (worldSize * 0.5f), worldSize);

		foreach (var chunkWorld in worldRect.GetChunkRectangles(UNIT_SIZE_WORLD, UNIT_SIZE_WORLD))
		{
			regions.Add(new DecalRegion(chunkWorld, DECAL_TEXTURE_SIZE));
		}

		Log.Send($"DecalSystem: {regions.Count} chunks ({DECAL_TEXTURE_SIZE}×{DECAL_TEXTURE_SIZE})");
	}

	public static void Paint(Vector2 worldPos)
	{
		foreach (var region in regions)
		{
			if (!Raylib.CheckCollisionCircleRec(worldPos, UNIT_SIZE_WORLD, region.WorldRect))
				continue;

			Vector2 localPos = (worldPos - region.WorldRect.Position) * DECAL_PPU;
			region.Paint(localPos);
		}
	}

	public static void Draw() //TODO: draw only visible in camera
	{
		foreach (var r in regions)
		{
			Raylib.DrawTexturePro(
				r.RenderTexture.Texture,
				new Rectangle(0, 0, r.RenderTexture.Texture.Width, -r.RenderTexture.Texture.Height),
				r.WorldRect,
				Vector2.Zero, 0, Color.White);
		}
	}

	public static void Dispose()
	{
		foreach (var i in regions) i.Dispose();
		regions.Clear();
	}
}