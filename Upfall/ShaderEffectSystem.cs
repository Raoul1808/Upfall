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
    private static float _circleWobbleSize;
    private static float _startCircleSize;
    private static float _targetCircleSize;
    private static float _currentCircleSize;
    private static float _circleTimer;

    public const float CircleTransitionTime = 0.5f;
    
    public override void PostInitialize(BroccoGame game)
    {
        _shader = Assets.GetEffect("DynamicOneBit");
        _shader.Parameters["CirclePos"].SetValue(Vector2.One * 200f);
    }

    public override void PostUpdate(GameTime gameTime)
    {
        const float wobbleAmount = 15f;
        
        float tt = (float)gameTime.TotalGameTime.TotalSeconds;
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (Math.Abs(_currentCircleSize - _targetCircleSize) > 0.0001f)
        {
            _circleTimer += dt;
            _currentCircleSize = MathHelper.Lerp(_startCircleSize, _targetCircleSize, (float)Easings.OutQuart(_circleTimer / CircleTransitionTime));
        }
        _circleWobbleSize = (float)Math.Sin(tt) * wobbleAmount - wobbleAmount;
        SetCircleRadius(_currentCircleSize + _circleWobbleSize);
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
        _shader.Parameters["CircleRadius"].SetValue(radius * _canvasRenderScale);
    }

    public static void SetDarkColor(Color color)
    {
        _shader.Parameters["BitColor1"].SetValue(color.ToVector4());
    }

    public static void SetLightColor(Color color)
    {
        _shader.Parameters["BitColor2"].SetValue(color.ToVector4());
    }

    public static void SetCircleRadiusAnim(float startRadius, float targetRadius)
    {
        if (Math.Abs(_targetCircleSize - targetRadius) < 0.0001f) return;
        _circleTimer = 0f;
        _startCircleSize = startRadius;
        _targetCircleSize = targetRadius;
    }

    public static void SetCircleCanvasPos(Vector2 pos) => SetCirclePos(GetScreenPosForCanvasPos(pos));

    public static Vector2 GetScreenPosForCanvasPos(Vector2 canvasPos)
    {
        return canvasPos * _canvasRenderScale + _canvasOffset;
    }
}
