using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Brocco;
using Brocco.Menu;
using FontStashSharp;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Upfall.Entities;

namespace Upfall.Scenes;

public class MenuScene : Scene
{
    private enum MenuState
    {
        MainMenu,
        OptionsMenu,
        LevelSetSelect,
        LevelSelect,
        LevelSetEnd,
    }

    private string LevelsDirectory => Path.Join(Assets.AssetsPath, "Levels");
    
    private MenuObject _mainMenu;
    private MenuObject _optionsMenu;
    private MenuObject _levelSetSelect;
    private MenuObject _levelSelect;
    private MenuObject _levelSetEnd;
    private RichTextLayout _levelSetEndText;
    private MenuState _currentMenu;

    private FontSystem _tinyUnicodeFont;

    private MenuSettings _menuSettings = new MenuSettings
    {
        FontSize = 64f,
        SelectEffect = MenuSelectEffect.Underline,
    };

    private Dictionary<string, List<string>> _detectedLevelSets;
    private string _selectedLevelSet;
    private int _selectedLevel;
    
    public override void Load()
    {
        AddToScene<TitleEntity>();
        
        ScreenEffect = Assets.GetEffect("DynamicOneBit");

        _tinyUnicodeFont = Assets.GetFontSystem("Tiny Unicode");
        
        _mainMenu = MenuBuilder.CreateMenu(_tinyUnicodeFont, UpfallCommon.ScreenCenter, _menuSettings)
            .AddButton("START", _ => ParseLevels())
            .AddButton("OPTIONS", _ => _currentMenu = MenuState.OptionsMenu)
            .AddButton("EDITOR", _ => SceneManager.Change("Editor"))
            .AddButton("EXIT", _ => ExitGame())
            .Build();

        _optionsMenu = MenuBuilder.CreateMenu(_tinyUnicodeFont, UpfallCommon.ScreenCenter, _menuSettings)
            .AddToggle("Display Custom Palettes", PaletteSystem.DisplayCustomPalettes, (_, enable) => PaletteSystem.DisplayCustomPalettes = enable)
            .AddArraySelect("Sound Volume", Enumerable.Range(0, 11).ToArray(), AudioManager.Volume, (_, volume) => AudioManager.Volume = volume)
            .AddButton("Back", _ => _currentMenu = MenuState.MainMenu)
            .Build();
    }

    public override void OnBecomeActive()
    {
        if (UpfallCommon.GetLevelSetName() != string.Empty)
        {
            _currentMenu = MenuState.LevelSetEnd;
            StringBuilder sb = new StringBuilder();
            var menuPos = UpfallCommon.ScreenCenter;
            sb.Append("Level Set " + UpfallCommon.GetLevelSetName() + " Complete!/n");
            sb.Append("Death Count: " + UpfallCommon.GetDeathCount());
            menuPos.Y += 128f + 64f;
            
            _levelSetEndText = new RichTextLayout()
            {
                Font = _tinyUnicodeFont.GetFont(64),
                Text = sb.ToString(),
            };

            _levelSetEnd = MenuBuilder.CreateMenu(_tinyUnicodeFont, menuPos, _menuSettings)
                .AddButton("Press Enter/A to continue", _ => _currentMenu = MenuState.MainMenu)
                .Build();
        }
        else
            _currentMenu = MenuState.MainMenu;
        UpfallCommon.InEditor = false;
        UpfallCommon.Playtesting = false;
        UpfallCommon.CurrentWorldMode = WorldMode.Dark;
        PaletteSystem.ResetPalette();
        ShaderEffectSystem.SetCircleRadiusAnim(0f, 0f);
        UpfallCommon.ResetPreviousLevelTextDisplayed();
        UpfallCommon.ResetDeathCount();
        UpfallCommon.LeaveLevelSet();
    }

    private MenuObject GetCurrentMenu()
    {
        return _currentMenu switch
        {
            MenuState.MainMenu => _mainMenu,
            MenuState.OptionsMenu => _optionsMenu,
            MenuState.LevelSetSelect => _levelSetSelect,
            MenuState.LevelSelect => _levelSelect,
            MenuState.LevelSetEnd => _levelSetEnd,
            _ => null,
        };
    }

    private void ParseLevels()
    {
        Dictionary<string, List<string>> levelsets = new();
        foreach (string directory in Directory.EnumerateDirectories(LevelsDirectory))
        {
            List<string> levels = new();
            foreach (string file in Directory.EnumerateFiles(directory))
            {
                if (file.EndsWith(".umd"))
                    levels.Add(file);
            }

            if (levels.Count > 0)
            {
                levels.Sort();
                string dirName = Path.GetFileName(directory);
                levelsets.Add(dirName, levels);
            }
        }

        var levelsetNames = levelsets.Keys.ToList();
        int officialSetIndex = levelsetNames.IndexOf("Upfall");
        _selectedLevelSet = "Upfall";
        _selectedLevel = 0;

        _levelSetSelect = MenuBuilder.CreateMenu(_tinyUnicodeFont, UpfallCommon.ScreenCenter, _menuSettings)
            .AddArraySelect("Level Set", levelsetNames.ToArray(), officialSetIndex, action: (_, set) => _selectedLevelSet = set)
            .AddButton("Start From Beginning", _ => StartLevel())
            .AddButton("Select Level", _ => ShowLevelSelection())
            .AddButton("Back", _ => _currentMenu = MenuState.MainMenu)
            .Build();

        _detectedLevelSets = levelsets;

        _currentMenu = MenuState.LevelSetSelect;
    }

    private void ShowLevelSelection()
    {
        _selectedLevel = 0;
        var levelSet = _detectedLevelSets[_selectedLevelSet];
        List<string> levels = new();
        foreach (string level in levelSet)
            levels.Add(Path.GetFileNameWithoutExtension(level));
        _levelSelect = MenuBuilder.CreateMenu(_tinyUnicodeFont, UpfallCommon.ScreenCenter, _menuSettings)
            .AddArraySelect("Level", levels.ToArray(), action: (_, level) => _selectedLevel = levels.IndexOf(level))
            .AddButton("Play", _ => StartLevel())
            .AddButton("Back", _ =>
            {
                _currentMenu = MenuState.LevelSetSelect;
                _selectedLevel = 0;
            })
            .Build();

        _currentMenu = MenuState.LevelSelect;
    }

    private void LevelSetEnd(SpriteBatch spriteBatch)
    {
        _levelSetEndText.Draw(spriteBatch, UpfallCommon.ScreenCenter, Color.White, horizontalAlignment: TextHorizontalAlignment.Center);
    }

    private void StartLevel()
    {
        UpfallCommon.SetLevelSet(_selectedLevelSet, _detectedLevelSets[_selectedLevelSet], _selectedLevel);
        SceneManager.Change("Game");
    }

    public override void Update(float dt)
    {
        GetCurrentMenu()?.Update();
    }

    public override void ScreenRender(SpriteBatch spriteBatch)
    {
        var font = _tinyUnicodeFont.GetFont(32);
        spriteBatch.DrawString(font, "v" + UpfallCommon.Version, Vector2.Zero, Color.White);
        GetCurrentMenu()?.Render(spriteBatch);
        if (_currentMenu == MenuState.LevelSetEnd)
            LevelSetEnd(spriteBatch);
    }
}
