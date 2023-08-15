using Brocco;
using Brocco.Basic;
using Microsoft.Xna.Framework;
using Upfall.Scenes;

namespace Upfall;

internal class Program
{
    public static void Main()
    {
        var game = new BroccoGame(new BroccoGameSettings
        {
            CanvasSize = new Size(640, 360),
            Resolution = new Size(1280, 720),
            ClearColor = Color.Black,
            ShowMouse = true,
        });
        
        Assets.PreloadFont("Open Sans", new []{"OpenSans.ttf"});
        
        game.AddSystem<NotificationSystem>();

        SceneManager.Add("Menu", new MenuScene());
        SceneManager.Add("Game", new GameScene());
        SceneManager.Add("Editor", new EditScene());

        game.Run();
    }
}
