using System;
using Brocco;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Upfall.Entities;

public class TitleEntity : Entity
{
    private Texture2D _letterU;
    private Texture2D _letterP;
    private Texture2D _letterF;
    private Texture2D _letterA;
    private Texture2D _letterL;

    private Vector2 _posU;
    private Vector2 _posP;
    private Vector2 _posF;
    private Vector2 _posA;
    private Vector2 _posL1;
    private Vector2 _posL2;

    private Vector2 _globalOffset;
    
    private float _timer;

    public TitleEntity()
    {
        const int gap = 6;
        
        _letterU = Assets.GetTexture("Title/title_u");
        _letterP = Assets.GetTexture("Title/title_p");
        _letterF = Assets.GetTexture("Title/title_f");
        _letterA = Assets.GetTexture("Title/title_a");
        _letterL = Assets.GetTexture("Title/title_l");

        int u = _letterU.Width;
        int p = _letterP.Width;
        int f = _letterP.Width;
        int a = _letterA.Width;
        int l = _letterL.Width;

        float height = UpfallCommon.CanvasCenter.Y / 2f;

        int totalWidth = _letterU.Width + _letterP.Width + _letterF.Width + _letterA.Width + _letterL.Width * 2 + gap * 5;
        _globalOffset = new Vector2(totalWidth / 2f, 15);

        _posU = new Vector2(0f, height);
        _posP = new Vector2(u + gap, height);
        _posF = new Vector2(u + gap + p + gap, height);
        _posA = new Vector2(u + gap + p + gap + f, height);
        _posL1 = new Vector2(u + gap + p + gap + f + a + gap, height);
        _posL2 = new Vector2(u + gap + p + gap + f + a + gap + l + gap, height);
    }
    
    public override void Update(float dt)
    {
        const float phase = -MathHelper.Pi / 8f;
        const float speed = 2f;
        const float strength = 8f;
        _timer += dt;
        if (_timer > MathHelper.TwoPi)
            _timer -= MathHelper.TwoPi;
        float height = UpfallCommon.CanvasCenter.Y / 2f;
        _posU.Y = strength * (float)Math.Sin(_timer * speed)  + height + 5f;
        _posP.Y = strength * (float)Math.Sin(_timer * speed + phase) + height;
        _posF.Y = strength * (float)Math.Sin(_timer * speed + phase * 2) + height;
        _posA.Y = strength * (float)Math.Sin(_timer * speed + phase * 3) + height - 5f;
        _posL1.Y = strength * (float)Math.Sin(_timer * speed + phase * 4) + height;
        _posL2.Y = strength * (float)Math.Sin(_timer * speed + phase * 5) + height;
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        var centerX = new Vector2(UpfallCommon.CanvasCenter.X, 0f);
        spriteBatch.Draw(_letterU, centerX + _posU, null, Color.White, 0f, _globalOffset, 1f, SpriteEffects.None, 0f);
        spriteBatch.Draw(_letterP, centerX + _posP, null, Color.White, 0f, _globalOffset, 1f, SpriteEffects.None, 0f);
        spriteBatch.Draw(_letterF, centerX + _posF, null, Color.White, 0f, _globalOffset, 1f, SpriteEffects.None, 0f);
        spriteBatch.Draw(_letterA, centerX + _posA, null, Color.White, 0f, _globalOffset, 1f, SpriteEffects.None, 0f);
        spriteBatch.Draw(_letterL, centerX + _posL1, null, Color.White, 0f, _globalOffset, 1f, SpriteEffects.None, 0f);
        spriteBatch.Draw(_letterL, centerX + _posL2, null, Color.White, 0f, _globalOffset, 1f, SpriteEffects.None, 0f);
    }
}
