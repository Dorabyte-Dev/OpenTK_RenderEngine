using GameEngine;
using OpenTK.Windowing.Desktop;

internal static class Program
{
    static void Main()
    {
        using (Game game = new Game(800, 600, "GameEngine"))
        {
            game.Run();
        }
    }
}
