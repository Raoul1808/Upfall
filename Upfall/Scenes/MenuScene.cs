using Brocco;
using Brocco.Menu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Upfall.Scenes;

public class MenuScene : Scene
{
    private MenuObject _mainMenu;
    
    public override void Load()
    {
        var menuSettings = new MenuSettings
        {
            FontSize = 24f,
        };
        _mainMenu = MenuBuilder.CreateMenu(Assets.GetFontSystem("Open Sans"), new Vector2(100f), menuSettings)
            .AddButton("Start", _ => SceneManager.Change("Game"))
            .AddButton("Options")
            .AddButton("Editor", _ => SceneManager.Change("Editor"))
            .AddButton("Exit", _ => ExitGame())
            .Build();
    }

    public override void OnBecomeActive()
    {
        UpfallCommon.InEditor = false;
    }

    public override void Update(float dt)
    {
        _mainMenu.Update();
    }

    public override void ScreenRender(SpriteBatch spriteBatch)
    {
        _mainMenu.Render(spriteBatch);
    }
}
