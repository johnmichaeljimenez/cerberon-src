using Main.Helpers;

namespace Main.Core;

public enum CursorType
{
    DefaultSystem,
    DefaultIngame,
    Interaction
}

public class InputButton
{
    public KeyboardKey? KeyButton;
    public MouseButton? MouseButton;

    public bool IsDown { get; private set; }
    public bool IsPressed { get; private set; }

    public InputButton(KeyboardKey? keyButton, MouseButton? mouseButton)
    {
        KeyButton = keyButton;
        MouseButton = mouseButton;
    }

    public void Update()
    {
        IsDown = (
            KeyButton.HasValue && Raylib.IsKeyDown(KeyButton.Value)) ||
            (MouseButton.HasValue && Raylib.IsMouseButtonDown(MouseButton.Value)
        );

        IsPressed |= ( //this "latch" mechanism made the render loop -> fixed loop synchronization work
            KeyButton.HasValue && Raylib.IsKeyPressed(KeyButton.Value)) ||
            (MouseButton.HasValue && Raylib.IsMouseButtonPressed(MouseButton.Value)
        );
    }

    public void LateUpdate()
    {
        IsDown = false;
        IsPressed = false;
    }
}

public enum InputAction
{
    None,
    Fire,
    AltFire,
    Interact,
    Weapon1,
    Weapon2,
    Weapon3,
    Weapon4,
    Reload,
    Flashlight,
    Nightvision,
    Pause
}

public static class InputManager
{
    const float DEADZONE = 0.15f;

    public static readonly PriorityCount CursorState = new()
    {
        OnChanged = OnCursorStateChanged
    };

    public static bool HasGamepad => Raylib.IsGamepadAvailable(0);
    public static Vector2 MousePosition { get; private set; }
    public static Vector2 MouseWorldPosition { get; private set; }

    public static Vector2 Movement { get; private set; }

    public static readonly List<InputAction> WeaponInputs = new()
    {
        InputAction.Weapon1,
        InputAction.Weapon2,
        InputAction.Weapon3,
        InputAction.Weapon4,
    };

    private static Dictionary<InputAction, InputButton> Actions = new()
    {
        { InputAction.Fire, new(null, MouseButton.Left) },
        { InputAction.AltFire, new(null, MouseButton.Right) },
        { InputAction.Interact, new(KeyboardKey.Space, null) },
        { InputAction.Flashlight, new(KeyboardKey.F, null) },
        { InputAction.Reload, new(KeyboardKey.R, null) },
        { InputAction.Weapon1, new(KeyboardKey.One, null) },
        { InputAction.Weapon2, new(KeyboardKey.Two, null) },
        { InputAction.Weapon3, new(KeyboardKey.Three, null) },
        { InputAction.Weapon4, new(KeyboardKey.Four, null) },
        { InputAction.Nightvision, new(KeyboardKey.N, null) },
        { InputAction.Pause, new(KeyboardKey.Escape, null) }
    };

    private static readonly Dictionary<CursorType, Sprite> cursorSprites = new();

    public static void Init()
    {
        cursorSprites[CursorType.DefaultSystem] = null;
        cursorSprites[CursorType.DefaultIngame] = AssetManager.GetSprite("ui/cursor-ingame");
        cursorSprites[CursorType.Interaction] = null;

        OnCursorStateChanged(0);
    }

    public static void Update(float scale, Vector2 offset, Camera2D camera)
    {
        Vector2 rawPos = Raylib.GetMousePosition();
        MousePosition = (rawPos - offset) / scale;

        MouseWorldPosition = Raylib.GetScreenToWorld2D(MousePosition, camera); //i dont care if there's "circular dependency" here if they are just about Game.cs and InputManager.cs and the benefit is 100% immediate usability in gameplay

        float gpX = HasGamepad ? Raylib.GetGamepadAxisMovement(0, GamepadAxis.LeftX) : 0f;
        float gpY = HasGamepad ? Raylib.GetGamepadAxisMovement(0, GamepadAxis.LeftY) : 0f;

        gpX = MathF.Abs(gpX) < DEADZONE ? 0f : gpX;
        gpY = MathF.Abs(gpY) < DEADZONE ? 0f : gpY;

        Vector2 dir = new Vector2
        {
            X = (Raylib.IsKeyDown(KeyboardKey.A) ? -1 : Raylib.IsKeyDown(KeyboardKey.D) ? 1 : 0) + gpX,
            Y = (Raylib.IsKeyDown(KeyboardKey.W) ? -1 : Raylib.IsKeyDown(KeyboardKey.S) ? 1 : 0) + gpY
        };

        Movement = (dir.X != 0 || dir.Y != 0) ? Raymath.Vector2Normalize(dir) : dir;

        foreach (var i in Actions)
        {
            i.Value.Update();
        }
    }

    public static void LateUpdate()
    {
        //consume all pending active inputs here
        foreach (var i in Actions)
        {
            i.Value.LateUpdate();
        }
    }

    public static bool IsPressed(InputAction action)
    {
        return Actions[action].IsPressed;
    }

    public static bool IsDown(InputAction action)
    {
        return Actions[action].IsDown;
    }

    public static void SetCursorState(string id, CursorType cursorType)
    {
        CursorState.Add(id, (int)cursorType);
    }

    public static void RemoveCursorState(string id)
    {
        CursorState.Remove(id);
    }

    public static void ClearCursorState()
    {
        CursorState.Clear();
    }

    private static void OnCursorStateChanged(int value)
    {
        var current = (CursorType)value;
        if (current == CursorType.DefaultSystem)
            Raylib.ShowCursor();
        else
            Raylib.HideCursor();
    }

    public static void DrawCursor()
    {
        var current = (CursorType)CursorState.Current;
        if (current == CursorType.DefaultSystem)
            return;

        var sprite = cursorSprites[current];
        if (sprite == null)
            return;

        Raylib.DrawTexturePro(sprite.Texture, sprite.Rect, new Rectangle(Raylib.GetMousePosition(), sprite.Rect.Size), sprite.Rect.Size * 0.5f, 0f, Color.White);
    }
}