using System;
using Brocco;
using Brocco.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Upfall.Entities;

public class Player : TilemapEntity
{
    enum FallDirection
    {
        Down = 1,
        Up = -1,
    }
    
    private const float MaxFallSpeed = 7.5f;
    private const float Gravity = 0.5f;
    private const float JumpForce = 7f;
    
    private const float HorizontalSpeed = 5f;
    private const float HorizontalAcceleration = 1f;

    private FallDirection _fallDirection = FallDirection.Down;
    private float _targetSpeed = 0;
    private bool _canJump = false;
    private bool _isJumping = false;
    
    public bool IsDead { get; private set; }
    public bool WonLevel { get; private set; }

    public Player()
    {
        CurrentTexture = Assets.GetTexture("player");
        IsDead = false;
        WonLevel = false;
        UpfallCommon.OnWorldChange += FlipGravity;
    }

    private FallDirection GetFallDirection(WorldMode mode)
    {
        return mode switch
        {
            WorldMode.Dark => FallDirection.Down,
            WorldMode.Light => FallDirection.Up,
            _ => FallDirection.Down,
        };
    }

    private void FlipGravity(WorldMode oldMode, WorldMode newMode)
    {
        _fallDirection = GetFallDirection(newMode);
        if (_fallDirection == FallDirection.Up)
            Flip |= SpriteEffects.FlipVertically;
        if (_fallDirection == FallDirection.Down)
            Flip &= ~SpriteEffects.FlipVertically;
    }

    public override void Update(float dt)
    {
        bool left = InputManager.GetKeyDown(Keys.Left) || InputManager.GetButtonDown(Buttons.LeftThumbstickLeft);
        bool right = InputManager.GetKeyDown(Keys.Right) || InputManager.GetButtonDown(Buttons.LeftThumbstickRight);
        bool jump = InputManager.GetKeyPress(Keys.Space) || InputManager.GetButtonPress(Buttons.A);
        bool jumping = InputManager.GetKeyDown(Keys.Space) || InputManager.GetButtonDown(Buttons.A);

        if (left)
            _targetSpeed = -HorizontalSpeed;
        if (right)
            _targetSpeed = HorizontalSpeed;
        if (left == right)
            _targetSpeed = 0;
        
        int direction = Math.Sign(_targetSpeed - Velocity.X);
        Velocity.X += HorizontalAcceleration * direction;
        if (Math.Sign(_targetSpeed - Velocity.X) != direction)
            Velocity.X = _targetSpeed;

        Velocity.Y += Gravity * (int)_fallDirection;
        if (Math.Abs(Velocity.Y) >= MaxFallSpeed)
            Velocity.Y = MaxFallSpeed * (int)_fallDirection;

        if (jump && _canJump)
        {
            Velocity.Y = -JumpForce * (int)_fallDirection;
            _isJumping = true;
            _canJump = false;
        }

        if (Velocity.Y == 0f || Math.Sign(Velocity.Y) == Math.Sign((int)_fallDirection))
            _isJumping = false;

        if (_isJumping && !jumping)
        {
            _isJumping = false;
            Velocity.Y *= 0.5f;
        }

        if (Velocity.X < 0)
            Flip |= SpriteEffects.FlipHorizontally;
        if (Velocity.X > 0)
            Flip &= ~SpriteEffects.FlipHorizontally;
    }

    private void OnLand()
    {
        _canJump = true;
        _isJumping = false;
    }

    private void OnTouchCeiling()
    {
        _isJumping = false;
    }

    public override void OnTileTopTouched(Rectangle tile)
    {
        switch (_fallDirection)
        {
            case FallDirection.Down:
                OnLand();
                break;

            case FallDirection.Up:
                OnTouchCeiling();
                break;
        }
    }

    public override void OnTileBottomTouched(Rectangle tile)
    {
        switch (_fallDirection)
        {
            case FallDirection.Down:
                OnTouchCeiling();
                break;

            case FallDirection.Up:
                OnLand();
                break;
        }
    }

    public override void Kill()
    {
        // 🦀 The player is dead 🦀
        IsDead = true;
        Dispose();
    }

    public void Win()
    {
        // Win code
        WonLevel = true;
        Velocity = Vector2.Zero;
    }
}
