using Main.Effects;

namespace Main.Core;

public class RenderingManager
{
    public class RendererFilter
    {
        public string ShaderLocationString;
        public int ShaderLocation;
        public float CurrentValue;
        public float TargetValue;

        public RendererFilter(string locString)
        {
            ShaderLocationString = locString;
        }

        public void Use(Shader shader)
        {
            CurrentValue = Raymath.Lerp(CurrentValue, TargetValue, Time.DeltaTime * 20);
            Raylib.SetShaderValue(shader, ShaderLocation, CurrentValue, ShaderUniformDataType.Float);
        }
    }

    public enum Filters
    {
        None,
        Nightvision
    }

    public const int VIRTUAL_WIDTH = 800; //hardcoded for now, might be actual 1080p by default (or at least 720p)
    public const int VIRTUAL_HEIGHT = 450;

    const string POST_FX = "Assets/Shaders/postfx.fs";

    public static Shader PostShader { get; private set; }
    public static Shader SpriteMasked { get; private set; }
    public static Shader SpriteTiled { get; private set; }

    private static int lightTexLoc;
    private static int visionTexLoc;
    private static int timeLoc;

    private static int maskVisionTexLoc;
    private static int tiledTexLocX, tiledTexLocY;

    public static float Scale;
    public static Vector2 Offset;

    private static readonly Dictionary<Filters, RendererFilter> AllFilters = new()
    {
        { Filters.Nightvision, new("nightAmt") }
    };

    public static void Init()
    {
        SpriteMasked = Raylib.LoadShader(null, "Assets/Shaders/sprite-masked.fs");
        SpriteTiled = Raylib.LoadShader(null, "Assets/Shaders/sprite-tiled.fs");

        maskVisionTexLoc = Raylib.GetShaderLocation(SpriteMasked, "visionTex");
        tiledTexLocX = Raylib.GetShaderLocation(SpriteTiled, "tilingX");
        tiledTexLocY = Raylib.GetShaderLocation(SpriteTiled, "tilingY");

        ReloadShader(AssetWatcher.Add(POST_FX, ReloadShader));
    }

    public static void BeginMaskedShader()
    {
        Raylib.BeginShaderMode(SpriteMasked);
        Raylib.SetShaderValueTexture(SpriteMasked, maskVisionTexLoc, LightingSystem.VisionRenderTexture.Texture);
    }

    public static void BeginTiledShader(Sprite sprite, Vector2 size)
    {
        var tiling = new Vector2(
                size.X * Sprite.PIXELS_PER_UNIT / (float)sprite.Texture.Width,
                size.Y * Sprite.PIXELS_PER_UNIT / (float)sprite.Texture.Height);

        Raylib.BeginShaderMode(SpriteTiled);
        Raylib.SetShaderValue(SpriteTiled, tiledTexLocX, tiling.X, ShaderUniformDataType.Float); //TODO: use vector2 (for some reason it doesnt work on that)
        Raylib.SetShaderValue(SpriteTiled, tiledTexLocY, tiling.Y, ShaderUniformDataType.Float);
    }

    private static void ReloadShader(string shader)
    {
        if (PostShader.Id != 0)
        {
            Raylib.UnloadShader(PostShader);
            PostShader = default;
        }

        PostShader = Raylib.LoadShaderFromMemory(null, shader);
        lightTexLoc = Raylib.GetShaderLocation(PostShader, "lightTex");
        visionTexLoc = Raylib.GetShaderLocation(PostShader, "visionTex");
        timeLoc = Raylib.GetShaderLocation(PostShader, "time");

        foreach (var i in AllFilters)
        {
            i.Value.ShaderLocation = Raylib.GetShaderLocation(PostShader, i.Value.ShaderLocationString);
        }
    }

    public static void UnloadPostShader()
    {
        AssetWatcher.Remove(POST_FX);

        if (SpriteTiled.Id != 0)
        {
            Raylib.UnloadShader(SpriteTiled);
            SpriteTiled = default;
        }

        if (SpriteMasked.Id != 0)
        {
            Raylib.UnloadShader(SpriteMasked);
            SpriteMasked = default;
        }

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
            Raylib.SetShaderValueTexture(PostShader, lightTexLoc, LightingSystem.LightingRenderTexture.Texture);
            Raylib.SetShaderValueTexture(PostShader, visionTexLoc, LightingSystem.VisionRenderTexture.Texture);
            Raylib.SetShaderValue(PostShader, timeLoc, Time.CurrentTime, ShaderUniformDataType.Float);

            foreach (var i in AllFilters)
            {
                i.Value.Use(PostShader);
            }
        }

        Rectangle source = new(0, 0, target.Texture.Width, -target.Texture.Height);
        Rectangle dest = new(Offset.X, Offset.Y, VIRTUAL_WIDTH * Scale, VIRTUAL_HEIGHT * Scale);
        Raylib.DrawTexturePro(target.Texture, source, dest, Vector2.Zero, 0.0f, Color.White);

        if (PostShader.Id != 0)
            Raylib.EndShaderMode();
    }

    public static void SetFilter(Filters filters, bool enabled)
    {
        AllFilters[filters].TargetValue = enabled ? 1 : 0;
    }
}