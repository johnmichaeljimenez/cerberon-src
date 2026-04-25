using Main.Effects;
using Main.Gameplay;

namespace Main.Core;

public interface IGameState
{
    void Enter();
    void Update(float dt, float udt);
    void Draw();
    void Exit();
    void DrawImGui();
}

public class Game
{
    public static Game Instance { get; private set; }

    private const int VirtualWidth = 800; //hardcoded for now, might be actual 1080p by default (or at least 720p)
    private const int VirtualHeight = 450;
    private RenderTexture2D _target;

    private IGameState currentState;
    private IGameState nextState;


    public CameraController Camera { get; private set; }

    public Game()
    {
        Instance = this;
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.VSyncHint);
        Raylib.InitWindow(VirtualWidth, VirtualHeight, "Raylib-cs Letterbox");
        Raylib.MaximizeWindow();
        _target = Raylib.LoadRenderTexture(VirtualWidth, VirtualHeight);
        RenderingManager.LoadPostShader();

        AssetManager.Init();

        Camera = new(VirtualWidth, VirtualHeight);
        LightingSystem.Init(VirtualWidth, VirtualHeight);

        rlImGui.Setup(true);

        currentState = new MenuState();
    }

    public void End()
    {
        currentState?.Exit();

        LightingSystem.Dispose();
        RenderingManager.UnloadPostShader();
        rlImGui.Shutdown();
        AssetManager.Dispose();
        Raylib.UnloadRenderTexture(_target);
        Raylib.CloseWindow();
    }

    private void Update(float dt, float udt, float scale, Vector2 offset)
    {
        currentState.Update(dt, udt);

        if (nextState != null)
        {
            currentState?.Exit();
            currentState = nextState;
            PauseHandler.Clear();
            currentState?.Enter();
            nextState = null;
        }

        Camera.Update(dt);
        RenderingManager.Update();

        InputManager.LateUpdate();
    }

    public void GoToIngame()
    {
        SetState(new GameplayState(new GameplayOptions()));
    }

    public void RestartGame()
    {
        SetState(new GameplayState(new()));
    }

    public void GoToMenu()
    {
        SetState(new MenuState());
    }

    private void SetState(IGameState state)
    {
        nextState = state;
    }

    private void Draw(float scale, Vector2 offset)
    {
        Raylib.BeginTextureMode(_target);
        {
            Raylib.ClearBackground(Color.Gray);

            Raylib.BeginMode2D(Camera.Camera);
            currentState.Draw();
            Raylib.EndMode2D();
        }
        Raylib.EndTextureMode();

        Raylib.BeginDrawing();
        {
            Raylib.ClearBackground(Color.Black);
            RenderingManager.DrawToScreen(_target, scale, offset, VirtualWidth, VirtualHeight);

            rlImGui.Begin();
            {
                Log.DrawImGui();

                if (ImGui.Begin("Debug"))
                {
                    ImGui.SeparatorText(currentState.GetType().Name);
                    currentState.DrawImGui();

                    ImGui.End();
                }

                if (ImGui.Begin("Assets"))
                {
                    ImGui.SeparatorText("Assets");
                    AssetManager.OnDrawImGui();

                    ImGui.End();
                }
            }
            rlImGui.End();

            Raylib.DrawFPS(4, 4);
        }
        Raylib.EndDrawing();
    }

    public void Run()
    {
        while (!Raylib.WindowShouldClose())
        {
            float scale = RenderingManager.GetScale(VirtualWidth, VirtualHeight);
            Vector2 offset = RenderingManager.GetOffset(VirtualWidth, VirtualHeight, scale);

            AudioHandler.Update();
            InputManager.Update(scale, offset, Camera.Camera); //press events are not captured reliably on 60hz loop, that's why it's here
            Time.Update((fixedDt, unscaledFixedDt) =>
            {
                Update(fixedDt, unscaledFixedDt, scale, offset);
            });

            Draw(scale, offset);
        }
    }
}