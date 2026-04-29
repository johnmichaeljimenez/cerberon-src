using Main.Effects;
using Main.Gameplay;
using Main.UI;

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
    public readonly Signal<IGameState> OnStateChanged = new();
    private RenderTexture2D _target;

    private IGameState currentState;
    private IGameState nextState;
    private bool requestExit;


    public CameraController Camera { get; private set; }

    private bool showIMGUI;

    public Game()
    {
        Instance = this;
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.VSyncHint);
        Raylib.InitWindow(RenderingManager.VIRTUAL_WIDTH, RenderingManager.VIRTUAL_HEIGHT, "Raylib-cs Letterbox");
        Raylib.MaximizeWindow();
        Raylib.SetExitKey(0);

        _target = Raylib.LoadRenderTexture(RenderingManager.VIRTUAL_WIDTH, RenderingManager.VIRTUAL_HEIGHT);
        RenderingManager.Init();

        AssetManager.Init();

        Camera = new(RenderingManager.VIRTUAL_WIDTH, RenderingManager.VIRTUAL_HEIGHT);
        LightingSystem.Init(RenderingManager.VIRTUAL_WIDTH, RenderingManager.VIRTUAL_HEIGHT);

        rlImGui.Setup(true);

        currentState = new MenuState();

        UIManager.Init();
    }

    public void End()
    {
        currentState?.Exit();
        UIManager.Dispose();

        LightingSystem.Dispose();
        RenderingManager.UnloadPostShader();
        rlImGui.Shutdown();
        AssetManager.Dispose();
        Raylib.UnloadRenderTexture(_target);
        AssetWatcher.Dispose();
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
            OnStateChanged?.Publish(currentState);
        }

        Camera.Update(dt);

        AssetWatcher.Update();
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

    private void Draw()
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
            RenderingManager.DrawToScreen(_target);
            UIManager.Draw();

            if (showIMGUI)
            {
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
            }

            Raylib.DrawFPS(4, 4);
        }
        Raylib.EndDrawing();
    }

    public void Run()
    {
        while (!Raylib.WindowShouldClose())
        {
            RenderingManager.UpdateLayout();
            float scale = RenderingManager.Scale;
            Vector2 offset = RenderingManager.Offset;

            AudioHandler.Update();
            InputManager.Update(scale, offset, Camera.Camera); //press events are not captured reliably on 60hz loop, that's why it's here
            Time.Update((fixedDt, unscaledFixedDt) =>
            {
                Update(fixedDt, unscaledFixedDt, scale, offset);
            });

            UIManager.Update(Time.DeltaTime, Time.UnscaledDeltaTime);

            if (Raylib.IsKeyPressed(KeyboardKey.F1))
                showIMGUI = !showIMGUI;

            Draw();

            if (requestExit)
                break;
        }
    }

    public void RequestExit()
    {
        requestExit = true;
    }
}