using System.Linq;
using Brocco;
using Brocco.Menu;
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
    
    public override void Load()
    {
        var menuSettings = new MenuSettings
        {
            FontSize = 64f,
            SelectEffect = MenuSelectEffect.Underline,
        };

        var tinyUnicode = Assets.GetFontSystem("Tiny Unicode");
        
        _mainMenu = MenuBuilder.CreateMenu(tinyUnicode, UpfallCommon.ScreenCenter, menuSettings)
            .AddButton("START", _ => SceneManager.Change("Game"))
            .AddButton("OPTIONS", _ => _currentMenu = MenuState.OptionsMenu)
            .AddButton("EDITOR", _ => SceneManager.Change("Editor"))
            .AddButton("EXIT", _ => ExitGame())
            .Build();

        _optionsMenu = MenuBuilder.CreateMenu(tinyUnicode, UpfallCommon.ScreenCenter, menuSettings)
            .AddToggle("Display Custom Palettes", true)
            .AddArraySelect("Sound Volume", Enumerable.Range(0, 11).ToArray(), 10)
            .AddButton("Back", _ => _currentMenu = MenuState.MainMenu)
            .Build();
    }

    public override void OnBecomeActive()
    {
        UpfallCommon.InEditor = false;
        UpfallCommon.Playtesting = false;
        UpfallCommon.CurrentWorldMode = WorldMode.Dark;
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
