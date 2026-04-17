using Main.Gameplay;

namespace Main.Core;

public interface IGameState
{
    void Enter();
    void Update(float dt);
    void Draw();
    void Exit();
    void DrawImGui();
}

public class Game
{
    public static Game Instance { get; private set; }

    private const int VirtualWidth = 800;
    private const int VirtualHeight = 450;
    private RenderTexture2D _target;

    private IGameState currentState;
    private IGameState nextState;


    public CameraController Camera { get; private set; }

    public Game()
    {
        Instance = this;
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        Raylib.InitWindow(800, 450, "Raylib-cs Letterbox");
        Raylib.MaximizeWindow();
        Raylib.SetTargetFPS(120);
        AssetManager.Init();
        Camera = new(VirtualWidth, VirtualHeight);

        rlImGui.Setup(true);

        _target = Raylib.LoadRenderTexture(VirtualWidth, VirtualHeight);

        currentState = new MenuState();
    }

    public void End()
    {
        rlImGui.Shutdown();
        AssetManager.Dispose();
        Raylib.UnloadRenderTexture(_target);
        Raylib.CloseWindow();
    }

    private void Update(float dt)
    {
        currentState.Update(dt);

        if (nextState != null)
        {
            currentState?.Exit();
            currentState = nextState;
            currentState?.Enter();
            nextState = null;
        }
    }

    public void GoToIngame()
    {
        SetState(new GameplayState(new GameplayOptions()));
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
            Raylib.ClearBackground(Color.RayWhite);

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
                ImGui.Begin("Debug");
                {
                    ImGui.SeparatorText(currentState.GetType().Name);
                    currentState.DrawImGui();

                    ImGui.SeparatorText("Assets");
                    AssetManager.OnDrawImGui();

                    ImGui.SliderFloat("Zoom", ref Camera.Camera.Zoom, 0.01f, 64f);
                }
                ImGui.End();
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
            InputManager.Update(scale, offset);
            Time.Update(Update);

            Draw(scale, offset);
        }
    }
}