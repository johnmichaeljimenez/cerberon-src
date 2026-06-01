using Main.Effects;
using Main.Gameplay;
using Main.Helpers;
using Main.UI;
using Tween;

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

    public bool IsIngame { get; private set; }


    public CameraController Camera { get; private set; }

    private GameplayOptions currentOptions;
    private bool showIMGUI;

    public Game()
    {
        Instance = this;
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.VSyncHint);
        Raylib.InitWindow(RenderingManager.VIRTUAL_WIDTH, RenderingManager.VIRTUAL_HEIGHT, "Vasodilator");
        Raylib.MaximizeWindow();
        Raylib.SetExitKey(0);

        DataConfigManager.Initialize();

        _target = Raylib.LoadRenderTexture(RenderingManager.VIRTUAL_WIDTH, RenderingManager.VIRTUAL_HEIGHT);
        RenderingManager.Init();

        AssetManager.Init();
        InputManager.Init();

        Camera = new(RenderingManager.VIRTUAL_WIDTH, RenderingManager.VIRTUAL_HEIGHT);
        LightingSystem.Init(RenderingManager.VIRTUAL_WIDTH, RenderingManager.VIRTUAL_HEIGHT);

        rlImGui.Setup(true);

        currentState = new MenuState();

        UIManager.Init();
        OnStateChanged?.Publish(currentState);


        Tween<float>.RegisterLerper(Raymath.Lerp);
        Tween<Vector2>.RegisterLerper(Raymath.Vector2Lerp);
        Tween<Vector3>.RegisterLerper(Raymath.Vector3Lerp);
        Tween<Color>.RegisterLerper(Colors.Lerp);
        Tween<Rectangle>.RegisterLerper((from, to, amt) => new Rectangle(
            Raymath.Lerp(from.X, to.X, amt),
            Raymath.Lerp(from.Y, to.Y, amt),
            Raymath.Lerp(from.Width, to.Width, amt),
            Raymath.Lerp(from.Height, to.Height, amt)
        ));
    }

    public void End()
    {
        TweenManager.Clear();
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
            TweenManager.Clear();
            PauseHandler.Clear();
            AudioHandler.StopMusic();
            InputManager.ClearCursorState();
            currentState?.Enter();
            nextState = null;
            OnStateChanged?.Publish(currentState);
        }

        Camera.Update(dt);

        AssetWatcher.Update();
        InputManager.LateUpdate();
    }

    public void GoToIngame(GameplayOptions gameplayOptions)
    {
        currentOptions = gameplayOptions ?? new GameplayOptions();
        SetState(new GameplayState(currentOptions));
    }

    public void RestartGame()
    {
        SetState(new GameplayState(currentOptions));
    }

    public void GoToMenu()
    {
        SetState(new MenuState());
    }

    private void SetState(IGameState state)
    {
        nextState = state;
        IsIngame = state is GameplayState;
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
            InputManager.DrawCursor();
            FadeHandler.Draw();

            if (showIMGUI)
            {
                rlImGui.Begin();
                {
                    Log.DrawImGui();

                    ImGui.Begin("Debug");
                    {
                        ImGui.SeparatorText(currentState.GetType().Name);
                        currentState.DrawImGui();
                    }
                    ImGui.End();

                    ImGui.Begin("Assets");
                    {
                        ImGui.SeparatorText("Assets");
                        AssetManager.OnDrawImGui();
                    }
                    ImGui.End();
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
            TweenManager.Update(Time.DeltaTime, Time.UnscaledDeltaTime);

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
            FadeHandler.Update();

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