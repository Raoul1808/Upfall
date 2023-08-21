using Brocco;
using Brocco.Input;
using FontStashSharp;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Upfall.Scenes;

public class SplashScene : Scene
{
    private FontSystem _font;
    private RichTextLayout _epilepsyText;
    
    public override void Load()
    {
        const string epilepsyWarning = "EPILEPSY WARNING\n" +
                                       "\n" +
                                       "This game features multiple palettes with\n" +
                                       "color transitions all over the place.\n" +
                                       "If you have any form of photosensitivity,\n" +
                                       "you should /tuIMMEDIATELY/td disable the\n" +
                                       "\"Display Custom Palettes\" option in the settings.\n" +
                                       "\n" +
                                       "Press Enter/A to continue";
        
        _font = Assets.GetFontSystem("Open Sans");
        _epilepsyText = new RichTextLayout
        {
            Text = epilepsyWarning,
            Font = _font.GetFont(48),
        };
    }

    public override void Update(float dt)
    {
        if (InputManager.GetKeyPress(Keys.Enter) || InputManager.GetButtonPress(Buttons.A))
            SceneManager.Change("Menu");
        base.Update(dt);
    }

    public override void ScreenRender(SpriteBatch spriteBatch)
    {
        var pos = new Vector2(UpfallCommon.ScreenCenter.X, 100f);
        _epilepsyText.Draw(spriteBatch, pos, Color.White, horizontalAlignment: TextHorizontalAlignment.Center);
        base.ScreenRender(spriteBatch);
    }
}
