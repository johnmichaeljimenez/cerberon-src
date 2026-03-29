using Main.Core;
using Raylib_cs;

namespace Main;

public class Program
{
    public static void Main()
    {
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        Raylib.InitWindow(800, 450, "Raylib-cs Letterbox");
        Raylib.MaximizeWindow();
        Raylib.SetTargetFPS(60);

        new Game().Run();

        Raylib.CloseWindow();
    }
}