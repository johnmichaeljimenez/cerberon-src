using Main.Core;
using Raylib_cs;

namespace Main;

public class Program
{
    [STAThread]
    public static void Main()
    {
        var game = new Game();
        game.Run();
        game.End();
    }
}