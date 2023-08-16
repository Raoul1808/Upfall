using System;
using Brocco;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Upfall;

public class ShaderEffectSystem : BroccoAutoSystem
{
    private static Effect _shader;
    
    public override void Initialize(BroccoGame game)
    {
        _shader = Assets.GetEffect("DynamicOneBit");
        _shader.Parameters["CirclePos"].SetValue(Vector2.One * 100f);
    }

    public override void PostUpdate(GameTime gameTime)
    {
        float dt = (float)gameTime.TotalGameTime.TotalSeconds;
        _shader.Parameters["CircleRadius"].SetValue((float)Math.Sin(dt) * 20f + 20f);
    }

    public static void SetCirclePos(Vector2 pos)
    {
        _shader.Parameters["CirclePos"].SetValue(pos / UpfallCommon.CanvasSize.ToVector2());
    }
}
