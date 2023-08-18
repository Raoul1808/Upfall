using System;
using Brocco;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Upfall;

public class NotificationSystem : BroccoAutoSystem
{
    private static FontSystem _openSansFont;
    private static FontSystem _tinyUnicodeFont;
    private static string _currentNotification = "";
    private static float _notificationTimer = 0f;

    private static string _levelName = "";
    private static string _levelAuthor = "";
    private static float _titleTimer = 0f;

    private static Vector2 _titlePos = new(UpfallCommon.ScreenCenter.X, UpfallCommon.ScreenCenter.Y * 2f - 50f);
    private static Vector2 _authorPos = new(_titlePos.X, _titlePos.Y + 32f);

    public override void PostInitialize(BroccoGame game)
    {
        _openSansFont = Assets.GetFontSystem("Open Sans");
        _tinyUnicodeFont = Assets.GetFontSystem("Tiny Unicode");
    }

    public override void PostUpdate(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_notificationTimer > 0f)
            _notificationTimer -= dt;
        if (_titleTimer > 0f)
            _titleTimer -= dt;
    }

    public override void Render(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (_notificationTimer <= 0f) return;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        var font = _openSansFont.GetFont(32);
        var alpha = Math.Clamp(_notificationTimer, 0f, 1f);
        spriteBatch.DrawString(font, _currentNotification, Vector2.One * 5f, Color.White * alpha,
            effect: FontSystemEffect.Stroked, effectAmount: 2);
        spriteBatch.End();
    }

    public static void SendNotification(string msg, float time = 2f)
    {
        _currentNotification = msg;
        _notificationTimer = time;
    }

    public static void ShowLevelName(string name, string author, float time = 2f)
    {
        _levelName = name;
        _levelAuthor = author;
        _titleTimer = time;
    }

    public static bool CanRenderName() => _titleTimer > 0f;

    public static void RenderName(SpriteBatch spriteBatch)
    {
        var titleFont = _tinyUnicodeFont.GetFont(48);
        var authorFont = _tinyUnicodeFont.GetFont(32);
        string author = "Created by " + _levelAuthor;
        var titleOffset = new Vector2(titleFont.MeasureString(_levelName).X / 2f, 48);
        var authorOffset = new Vector2(authorFont.MeasureString(author).X / 2f, 32);
        spriteBatch.DrawString(titleFont, _levelName, _titlePos, Color.White, origin: titleOffset, effect: FontSystemEffect.Stroked, effectAmount: 2);
        spriteBatch.DrawString(authorFont, author, _authorPos, Color.White, origin: authorOffset, effect: FontSystemEffect.Stroked, effectAmount: 2);
    }
}
