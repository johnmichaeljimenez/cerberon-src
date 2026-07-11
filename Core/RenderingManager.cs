using Cerberon.Effects;
using Cerberon.Gameplay;
using Tween;

namespace Cerberon.Core;

public static class RenderingManager
{
    public class ShaderSet : IDisposable
    {
        public Shader Shader;
        public readonly Dictionary<string, int> Keys = new();

        private string path;

        public ShaderSet(string path, params string[] keys)
        {
            this.path = $"Assets/Shaders/{path}.fs";
            foreach (var i in keys)
            {
                Keys.Add(i, -1);
            }
        }

        public void Init()
        {
            Shader = Raylib.LoadShader(null, path);
            foreach (var i in Keys)
            {
                Keys[i.Key] = Raylib.GetShaderLocation(Shader, i.Key);
            }
        }

        public void Dispose()
        {
            if (Raylib.IsShaderValid(Shader))
            {
                Raylib.UnloadShader(Shader);
                Shader = default;
            }
        }

        public void Begin()
        {
            Raylib.BeginShaderMode(Shader);
        }

        public void SetValue(string key, float t)
        {
            Raylib.SetShaderValue(Shader, Keys[key], t, ShaderUniformDataType.Float);
        }

        public void SetValue(string key, Texture2D texture)
        {
            Raylib.SetShaderValueTexture(Shader, Keys[key], texture);
        }

        public void SetValue(string key, Vector2 t)
        {
            Raylib.SetShaderValue(Shader, Keys[key], t, ShaderUniformDataType.Vec2);
        }

        public void SetValue(string keyX, string keyY, Vector2 t)
        {
            Raylib.SetShaderValue(Shader, Keys[keyX], t.X, ShaderUniformDataType.Float); //TODO: use vector2 (for some reason it doesnt work on that)
            Raylib.SetShaderValue(Shader, Keys[keyY], t.Y, ShaderUniformDataType.Float);
        }

        public void SetValue(string key, int t)
        {
            Raylib.SetShaderValue(Shader, Keys[key], t, ShaderUniformDataType.Int);
        }

        public void SetValue(string key, bool t)
        {
            Raylib.SetShaderValue(Shader, Keys[key], t ? 1 : 0, ShaderUniformDataType.Int);
        }
    }

    public class RendererFilter
    {
        public string ShaderLocationString;
        public readonly string TweenID;
        public int ShaderLocation;
        public float CurrentValue;
        public float TargetValue;

        public RendererFilter(string locString)
        {
            ShaderLocationString = locString;
            TweenID = $"{TWEEN_KEY}-{locString}";
        }

        public void Play(float from, float to, float duration, EasingFunction easing = null)
        {
            TweenManager.Add(new Tween<float>(() => CurrentValue, n => CurrentValue = n, to, duration, from, TweenID, true).SetEasing(easing ?? Easing.QuadInOut));
        }

        public void Use(Shader shader)
        {
            Raylib.SetShaderValue(shader, ShaderLocation, CurrentValue, ShaderUniformDataType.Float);
        }
    }

    public enum Filters
    {
        None,
        Nightvision,
        Hurt
    }

    public const int VIRTUAL_WIDTH = 800; //hardcoded for now, might be actual 1080p by default (or at least 720p)
    public const int VIRTUAL_HEIGHT = 450;

    public const string TWEEN_KEY = "Filter";
    const string POST_FX = "Assets/Shaders/postfx.fs";

    public static Shader PostShader { get; private set; }

    private static int lightTexLoc;
    private static int visionTexLoc;
    private static int timeLoc;
    private static int fadeLoc;
    private static int dirtyLensLoc;

    public static float Scale;
    public static Vector2 Offset;

    private static float fadeAmount;

    private static Texture2D dirtyLensTexture;

    public static readonly Dictionary<string, ShaderSet> ShaderSets = new()
    {
        { "SpriteEntity", new("sprite-entity", "visionTex", "maskAmount") },
        { "SpriteEnvironment", new("sprite-environment", "eraseVision", "visionTex", "tilingX", "tilingY", "tilingMode", "stochasticMode") },
        { "Blur", new("blur", "resolutionX", "resolutionY") }
    };

    private static readonly Dictionary<Filters, RendererFilter> AllFilters = new()
    {
        { Filters.Nightvision, new("nightAmt") },
        { Filters.Hurt, new("hurtAmt") }
    };

    public static void Init()
    {
        foreach (var i in ShaderSets)
        {
            i.Value.Init();
        }

        ReloadShader(AssetWatcher.Add(POST_FX, ReloadShader));

        dirtyLensTexture = AssetManager.GetSprite("dirty-lens").Texture;
        TweenManager.ClearByPrefix(TWEEN_KEY);
    }

    public static ShaderSet BeginEntityShader()
    {
        var shaderSet = ShaderSets["SpriteEntity"];

        shaderSet.Begin();
        shaderSet.SetValue("visionTex", LightingSystem.VisionRenderTexture.Texture);

        return shaderSet;
    }

    public static ShaderSet BeginEnvironmentShader(bool tiling, bool eraseVision, Sprite sprite, Vector2 size)
    {
        var shaderSet = ShaderSets["SpriteEnvironment"];

        shaderSet.Begin();
        shaderSet.SetValue("visionTex", LightingSystem.VisionRenderTexture.Texture);
        shaderSet.SetValue("eraseVision", eraseVision);
        shaderSet.SetValue("tilingMode", tiling);
        shaderSet.SetValue("stochasticMode", tiling && sprite.Metadata.StochasticTiling);

        var tile = new Vector2(
                size.X * Sprite.PIXELS_PER_UNIT / (float)sprite.Texture.Width,
                size.Y * Sprite.PIXELS_PER_UNIT / (float)sprite.Texture.Height);

        shaderSet.SetValue("tilingX", "tilingY", tile);

        return shaderSet;
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
        fadeLoc = Raylib.GetShaderLocation(PostShader, "fadeAmt");
        dirtyLensLoc = Raylib.GetShaderLocation(PostShader, "dirtyLensTex");

        foreach (var i in AllFilters)
        {
            i.Value.ShaderLocation = Raylib.GetShaderLocation(PostShader, i.Value.ShaderLocationString);
        }
    }

    public static void UnloadPostShader()
    {
        AssetWatcher.Remove(POST_FX);

        foreach (var i in ShaderSets)
        {
            i.Value.Dispose();
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
        var drawPost = PostShader.Id != 0 && !GameplayState.EnableDrawDebug && Game.Instance.IsIngame;
        if (drawPost)
        {
            LightingSystem.Draw();
            Raylib.BeginShaderMode(PostShader);
            Raylib.SetShaderValueTexture(PostShader, lightTexLoc, LightingSystem.LightingRenderTexture.Texture);
            Raylib.SetShaderValueTexture(PostShader, visionTexLoc, LightingSystem.VisionRenderTexture.Texture);
            Raylib.SetShaderValue(PostShader, timeLoc, Time.UnscaledCurrentTime, ShaderUniformDataType.Float);
            Raylib.SetShaderValue(PostShader, fadeLoc, fadeAmount, ShaderUniformDataType.Float);
            Raylib.SetShaderValueTexture(PostShader, dirtyLensLoc, dirtyLensTexture);

            foreach (var i in AllFilters)
            {
                i.Value.Use(PostShader);
            }
        }

        Rectangle source = new(0, 0, target.Texture.Width, -target.Texture.Height);
        Rectangle dest = new(Offset.X, Offset.Y, VIRTUAL_WIDTH * Scale, VIRTUAL_HEIGHT * Scale);
        Raylib.DrawTexturePro(target.Texture, source, dest, Vector2.Zero, 0.0f, Color.White);

        if (drawPost)
            Raylib.EndShaderMode();
    }

    public static void SetFilter(Filters filters, bool enabled, float duration = 0.2f, EasingFunction easing = null)
    {
        AllFilters[filters].Play(enabled ? 0 : 1, enabled ? 1 : 0, duration, easing);
    }

    public static void ResetAllFilters()
    {
        TweenManager.ClearByPrefix(TWEEN_KEY);
        foreach (var i in AllFilters)
        {
            i.Value.TargetValue = 0;
            i.Value.CurrentValue = 0;
        }
    }

    public static void SetFade(float amt)
    {
        fadeAmount = amt;
    }
}