using System.Numerics;
using Raylib_cs;

namespace Main.Core;

public class RenderingManager
{
    public static float GetScale(int virtualWidth, int virtualHeight) =>
        Math.Min((float)Raylib.GetScreenWidth() / virtualWidth, (float)Raylib.GetScreenHeight() / virtualHeight);

    public static Vector2 GetOffset(int virtualWidth, int virtualHeight, float scale) =>
        new((Raylib.GetScreenWidth() - (virtualWidth * scale)) * 0.5f, (Raylib.GetScreenHeight() - (virtualHeight * scale)) * 0.5f);

    public static void DrawToScreen(RenderTexture2D target, float scale, Vector2 offset, int virtualWidth, int virtualHeight)
    {
        Rectangle source = new(0, 0, target.Texture.Width, -target.Texture.Height);
        Rectangle dest = new(offset.X, offset.Y, virtualWidth * scale, virtualHeight * scale);
        Raylib.DrawTexturePro(target.Texture, source, dest, Vector2.Zero, 0.0f, Color.White);
    }
}