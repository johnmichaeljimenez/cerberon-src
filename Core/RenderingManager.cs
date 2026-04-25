using Main.Effects;

namespace Main.Core;

public class RenderingManager
{
    public const int VIRTUAL_WIDTH = 800; //hardcoded for now, might be actual 1080p by default (or at least 720p)
    public const int VIRTUAL_HEIGHT = 450;

    const string POST_FX = "Assets/Shaders/postfx.fs";

    public static Shader PostShader { get; private set; }

    private static int lightTextLoc;
    private static int visionTextLoc;

    public static float Scale;
    public static Vector2 Offset;

    public static void LoadPostShader()
    {
        ReloadShader(AssetWatcher.Add(POST_FX, ReloadShader));
    }

    private static void ReloadShader(string shader)
    {
        if (PostShader.Id != 0)
        {
            Raylib.UnloadShader(PostShader);
            PostShader = default;
        }

        PostShader = Raylib.LoadShaderFromMemory(null, shader);
        lightTextLoc = Raylib.GetShaderLocation(PostShader, "lightTex");
        visionTextLoc = Raylib.GetShaderLocation(PostShader, "visionTex");
    }

    public static void UnloadPostShader()
    {
        AssetWatcher.Remove(POST_FX);

        if (PostShader.Id != 0)
        {
            Raylib.UnloadShader(PostShader);
            PostShader = default;
        }
    }

    public static Rectangle GetRect(Vector2 pos, Vector2 size)
    {
        var scaledPos = pos * Scale;
        var scaledSize = size * Scale;
        
        scaledPos += Offset;
        // scaledPos = new Vector2(MathF.Round(scaledPos.X), MathF.Round(scaledPos.Y));
        // scaledSize = new Vector2(MathF.Round(scaledSize.X), MathF.Round(scaledSize.Y));

        return new Rectangle(scaledPos, scaledSize);
    }

    public static void UpdateLayout()
    {
        Scale = Math.Min((float)Raylib.GetScreenWidth() / VIRTUAL_WIDTH, (float)Raylib.GetScreenHeight() / VIRTUAL_HEIGHT);
        Offset = new((Raylib.GetScreenWidth() - (VIRTUAL_WIDTH * Scale)) * 0.5f, (Raylib.GetScreenHeight() - (VIRTUAL_HEIGHT * Scale)) * 0.5f);
    }

    public static void DrawToScreen(RenderTexture2D target)
    {
        if (PostShader.Id != 0)
        {
            LightingSystem.Draw();
            Raylib.BeginShaderMode(PostShader);
            Raylib.SetShaderValueTexture(PostShader, lightTextLoc, LightingSystem.LightingRenderTexture.Texture);
            Raylib.SetShaderValueTexture(PostShader, visionTextLoc, LightingSystem.VisionRenderTexture.Texture);
        }

        Rectangle source = new(0, 0, target.Texture.Width, -target.Texture.Height);
        Rectangle dest = new(Offset.X, Offset.Y, VIRTUAL_WIDTH * Scale, VIRTUAL_HEIGHT * Scale);
        Raylib.DrawTexturePro(target.Texture, source, dest, Vector2.Zero, 0.0f, Color.White);

        if (PostShader.Id != 0)
            Raylib.EndShaderMode();
    }
}