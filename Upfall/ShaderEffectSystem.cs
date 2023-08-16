using System;
using Brocco;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Upfall;

public class ShaderEffectSystem : BroccoAutoSystem
{
    private static Effect _shader;
    private static float _canvasRenderScale;
    private static Vector2 _canvasOffset;
    
    public override void Initialize(BroccoGame game)
    {
        _shader = Assets.GetEffect("DynamicOneBit");
        _shader.Parameters["CirclePos"].SetValue(Vector2.One * 200f);
    }

    public override void PostUpdate(GameTime gameTime)
    {
        float dt = (float)gameTime.TotalGameTime.TotalSeconds;
        SetCircleRadius((float)Math.Sin(dt) * 10f + 50f);
    }

    public override void OnGameResize(GameResizeEvent oldState, GameResizeEvent newState)
    {
        _canvasOffset = newState.CanvasOffset;
        _canvasRenderScale = newState.CanvasRenderScale;
    }

    public static void SetCirclePos(Vector2 pos)
    {
        _shader.Parameters["CirclePos"].SetValue(pos);
    }

    public static void SetCircleRadius(float radius)
    {
        _shader.Parameters["CircleRadius"].SetValue(radius);
    }

    public static void SetCircleCanvasPos(Vector2 pos) => SetCirclePos(GetScreenPosForCanvasPos(pos));

    public static Vector2 GetScreenPosForCanvasPos(Vector2 canvasPos)
    {
        return canvasPos * _canvasRenderScale + _canvasOffset;
    }
}
