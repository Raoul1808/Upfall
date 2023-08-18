using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Brocco;
using Brocco.Menu;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Upfall.Scenes;

public class MenuScene : Scene
{
    private enum MenuState
    {
        MainMenu,
        OptionsMenu,
        LevelSetSelect,
        LevelSelect,
    }

    private string LevelsDirectory => Path.Join(Assets.AssetsPath, "Levels");
    
    private MenuObject _mainMenu;
    private MenuObject _optionsMenu;
    private MenuObject _levelSetSelect;
    private MenuObject _levelSelect;
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
        _currentMenu = MenuState.MainMenu;
        UpfallCommon.InEditor = false;
        UpfallCommon.Playtesting = false;
        UpfallCommon.CurrentWorldMode = WorldMode.Dark;
        PaletteSystem.ResetPalette();
        ShaderEffectSystem.SetCircleRadius(0f);
    }

    private MenuObject GetCurrentMenu()
    {
        return _currentMenu switch
        {
            MenuState.MainMenu => _mainMenu,
            MenuState.OptionsMenu => _optionsMenu,
            MenuState.LevelSetSelect => _levelSetSelect,
            MenuState.LevelSelect => _levelSelect,
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
                levels.Add(file);
            }

            if (levels.Count > 0)
            {
                string dirName = Path.GetFileName(directory);
                levelsets.Add(dirName, levels);
            }
        }

        var levelsetNames = levelsets.Keys.ToList();
        int officialSetIndex = levelsetNames.IndexOf("Official");
        _selectedLevelSet = "Official";

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
        var levelSet = _detectedLevelSets[_selectedLevelSet];
        _levelSelect = MenuBuilder.CreateMenu(_tinyUnicodeFont, UpfallCommon.ScreenCenter, _menuSettings)
            .AddArraySelect("Level", levelSet.ToArray(),
                action: (_, level) => _selectedLevel = levelSet.IndexOf(level))
            .AddButton("Play", _ => StartLevel())
            .AddButton("Back", _ => _currentMenu = MenuState.LevelSetSelect)
            .Build();

        _currentMenu = MenuState.LevelSelect;
    }

    private void StartLevel()
    {
        UpfallCommon.SetLevelSet(_detectedLevelSets[_selectedLevelSet], _selectedLevel);
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
    }
}
