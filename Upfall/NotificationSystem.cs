using System;
using Brocco;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Upfall;

public class NotificationSystem : BroccoAutoSystem
{
    private static FontSystem _font;
    private static string _currentNotification = "";
    private static float _notificationTimer = 0f;

    public override void Initialize(BroccoGame game)
    {
        _font = Assets.GetFontSystem("Open Sans");
    }

    public override void PostUpdate(GameTime gameTime)
    {
        if (_notificationTimer > 0f)
            _notificationTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    public override void Render(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (_notificationTimer <= 0f)
            return;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        var font = _font.GetFont(32);
        var alpha = Math.Clamp(_notificationTimer, 0f, 1f);
        spriteBatch.DrawString(font, _currentNotification, Vector2.One * 5f, Color.White * alpha, effect: FontSystemEffect.Stroked, effectAmount: 2);
        spriteBatch.End();
    }

    public static void SendNotification(string msg, float time = 2f)
    {
        _currentNotification = msg;
        _notificationTimer = time;
    }
}
