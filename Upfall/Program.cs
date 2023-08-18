﻿using Brocco;
using Brocco.Basic;
using Microsoft.Xna.Framework;
using Upfall.Scenes;

namespace Upfall;

internal class Program
{
    public static void Main()
    {
        var gameSettings = new BroccoGameSettings
        {
            CanvasSize = new Size(640, 368),
            Resolution = new Size(1280, 736),
            ClearColor = Color.Black,
            ShowMouse = true,
        };
        using var game = new BroccoGame(gameSettings);
        game.Window.Title = "Upfall v" + UpfallCommon.ShortVersion;

        UpfallCommon.ScreenCenter = gameSettings.Resolution?.ToVector2() / 2f ?? new Vector2();  // wtf

        Assets.FontsRoot = "Fonts";
        Assets.SoundsRoot = "Sounds";
        Assets.TexturesRoot = "Textures";
        
        Assets.PreloadFont("Open Sans", new []{"OpenSans.ttf"});
        Assets.PreloadFont("Tiny Unicode", new[]{"TinyUnicode.ttf"});
        
        game.AddSystem<NotificationSystem>();
        game.AddSystem<ShaderEffectSystem>();
        game.AddSystem<PaletteSystem>();

        SceneManager.Add("Splash", new SplashScene());
        SceneManager.Add("Menu", new MenuScene());
        SceneManager.Add("Game", new GameScene());
        SceneManager.Add("Editor", new EditScene());

        game.Run();
    }
}
