using System.IO;
using System.Reflection;
using Brocco;
using Brocco.Basic;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Upfall.Scenes;

namespace Upfall;

internal class Program
{
    private struct GameSettings
    {
        public bool DisplayCustomPalettes { get; set; }
        public int AudioVolume { get; set; }

        public GameSettings()
        {
            DisplayCustomPalettes = true;
            AudioVolume = 10;
        }
    }

    private const string SettingsFile = "Settings.json";
    private static readonly string ConfigLocation = Path.Join(UpfallCommon.GamePath, SettingsFile);
    
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
        UpfallCommon.CanvasCenter = gameSettings.CanvasSize.ToVector2() / 2f;

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

        GameSettings settings;
        try
        {
            settings = JsonConvert.DeserializeObject<GameSettings>(File.ReadAllText(ConfigLocation));
        }
        catch
        {
            settings = new GameSettings();
        }

        AudioManager.Volume = settings.AudioVolume;
        PaletteSystem.PreSetDisplayPaletteOption(settings.DisplayCustomPalettes);

        game.Run();

        settings.AudioVolume = AudioManager.Volume;
        settings.DisplayCustomPalettes = PaletteSystem.DisplayCustomPalettes;

        File.WriteAllText(ConfigLocation, JsonConvert.SerializeObject(settings));
    }
}
