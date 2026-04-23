using Main.Effects;

namespace Main.Core;

public class RenderingManager
{
    const string POST_FX = "Assets/Shaders/postfx.fs";

    private static FileSystemWatcher? _shaderWatcher;
    private static bool _shaderReloadRequested;

    public static Shader PostShader { get; private set; }

    public static float GetScale(int virtualWidth, int virtualHeight) =>
        Math.Min((float)Raylib.GetScreenWidth() / virtualWidth, (float)Raylib.GetScreenHeight() / virtualHeight);

    public static Vector2 GetOffset(int virtualWidth, int virtualHeight, float scale) =>
        new((Raylib.GetScreenWidth() - (virtualWidth * scale)) * 0.5f, (Raylib.GetScreenHeight() - (virtualHeight * scale)) * 0.5f);


    private static int lightTextLoc;
    private static int visionTextLoc;

    public static void LoadPostShader()
    {
        SetupShaderWatcher();
        ReloadShader();
    }

    private static void SetupShaderWatcher() //hot-reload for super fast iteration
    {
        if (_shaderWatcher != null) return;

        string relativePath = POST_FX;
        string fullPath = Path.GetFullPath(relativePath);
        string directory = Path.GetDirectoryName(fullPath)!;
        string fileName = Path.GetFileName(fullPath);

        _shaderWatcher = new FileSystemWatcher(directory, fileName)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Size,
            EnableRaisingEvents = true
        };

        _shaderWatcher.Changed += OnShaderChanged;
    }

    private static void OnShaderChanged(object sender, FileSystemEventArgs e)
    {
        _shaderReloadRequested = true;
    }

    private static void ReloadShader()
    {
        if (PostShader.Id != 0)
        {
            Raylib.UnloadShader(PostShader);
            PostShader = default;
        }

        PostShader = Raylib.LoadShader(null, POST_FX);
        lightTextLoc = Raylib.GetShaderLocation(PostShader, "lightTex");
        visionTextLoc = Raylib.GetShaderLocation(PostShader, "visionTex");
    }

    public static void UnloadPostShader()
    {
        if (_shaderWatcher != null)
        {
            _shaderWatcher.EnableRaisingEvents = false;
            _shaderWatcher.Dispose();
            _shaderWatcher = null;
        }

        if (PostShader.Id != 0)
        {
            Raylib.UnloadShader(PostShader);
            PostShader = default;
        }
    }

    public static void Update()
    {
        if (_shaderReloadRequested)
        {
            _shaderReloadRequested = false; //threading-related stuff
            ReloadShader();
        }
    }

    public static void DrawToScreen(RenderTexture2D target, float scale, Vector2 offset, int virtualWidth, int virtualHeight)
    {
        if (PostShader.Id != 0)
        {
            LightingSystem.Draw();
            Raylib.BeginShaderMode(PostShader);
            Raylib.SetShaderValueTexture(PostShader, lightTextLoc, LightingSystem.LightingRenderTexture.Texture);
            Raylib.SetShaderValueTexture(PostShader, visionTextLoc, LightingSystem.VisionRenderTexture.Texture);
        }

        Rectangle source = new(0, 0, target.Texture.Width, -target.Texture.Height);
        Rectangle dest = new(offset.X, offset.Y, virtualWidth * scale, virtualHeight * scale);
        Raylib.DrawTexturePro(target.Texture, source, dest, Vector2.Zero, 0.0f, Color.White);

        if (PostShader.Id != 0)
            Raylib.EndShaderMode();
    }
}