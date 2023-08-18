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
    }
    
    private MenuObject _mainMenu;
    private MenuObject _optionsMenu;
    private MenuState _currentMenu;

    private FontSystem _tinyUnicodeFont;
    
    public override void Load()
    {
        ScreenEffect = Assets.GetEffect("DynamicOneBit");
        
        var menuSettings = new MenuSettings
        {
            FontSize = 64f,
            SelectEffect = MenuSelectEffect.Underline,
        };

        _tinyUnicodeFont = Assets.GetFontSystem("Tiny Unicode");
        
        _mainMenu = MenuBuilder.CreateMenu(_tinyUnicodeFont, UpfallCommon.ScreenCenter, menuSettings)
            .AddButton("START", _ => SceneManager.Change("Game"))
            .AddButton("OPTIONS", _ => _currentMenu = MenuState.OptionsMenu)
            .AddButton("EDITOR", _ => SceneManager.Change("Editor"))
            .AddButton("EXIT", _ => ExitGame())
            .Build();

        _optionsMenu = MenuBuilder.CreateMenu(_tinyUnicodeFont, UpfallCommon.ScreenCenter, menuSettings)
            .AddToggle("Display Custom Palettes", PaletteSystem.DisplayCustomPalettes, (_, enable) => PaletteSystem.DisplayCustomPalettes = enable)
            .AddArraySelect("Sound Volume", Enumerable.Range(0, 11).ToArray(), AudioManager.Volume, (_, volume) => AudioManager.Volume = volume)
            .AddButton("Back", _ => _currentMenu = MenuState.MainMenu)
            .Build();
    }

    public override void OnBecomeActive()
    {
        UpfallCommon.InEditor = false;
        UpfallCommon.Playtesting = false;
        UpfallCommon.CurrentWorldMode = WorldMode.Dark;
        PaletteSystem.ResetPalette();
        ShaderEffectSystem.SetCircleRadius(0f);
    }

    public override void Update(float dt)
    {
        switch (_currentMenu)
        {
            case MenuState.MainMenu:
                _mainMenu.Update();
                break;

            case MenuState.OptionsMenu:
                _optionsMenu.Update();
                break;
        }
    }

    public override void ScreenRender(SpriteBatch spriteBatch)
    {
        var font = _tinyUnicodeFont.GetFont(32);
        spriteBatch.DrawString(font, "v" + UpfallCommon.Version, Vector2.Zero, Color.White);
        switch (_currentMenu)
        {
            case MenuState.MainMenu:
                _mainMenu.Render(spriteBatch);
                break;

            case MenuState.OptionsMenu:
                _optionsMenu.Render(spriteBatch);
                break;
        }
    }
}
