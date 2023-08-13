using Brocco;
using Brocco.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Upfall.Entities;

internal class Player : Entity
{
    private const float MaxFallSpeed = 40f;
    private const float Gravity = 20f;
    private const float HorizontalSpeed = 5f;
    private const float JumpForce = 7f;

    private Vector2 _velocity;
    
    public Player()
    {
        CurrentTexture = Assets.GetTexture("player");
    }
    
    public override void Update(float dt)
    {
        bool left = InputManager.GetKeyDown(Keys.Left) || InputManager.GetButtonDown(Buttons.LeftThumbstickLeft);
        bool right = InputManager.GetKeyDown(Keys.Right) || InputManager.GetButtonDown(Buttons.LeftThumbstickRight);
        bool jump = InputManager.GetKeyPress(Keys.Space) || InputManager.GetButtonPress(Buttons.A);

        if (left)
            _velocity.X = -HorizontalSpeed;
        if (right)
            _velocity.X = HorizontalSpeed;
        if (left == right)
            _velocity.X = 0;

        _velocity.Y += Gravity * dt;
        if (_velocity.Y >= MaxFallSpeed)
            _velocity.Y = MaxFallSpeed;

        if (jump)
            _velocity.Y = -JumpForce;

        Position += _velocity;
    }
}
