using Brocco;
using Brocco.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Upfall.Entities;

internal class Player : Entity
{
    private Vector2 _velocity;
    
    public Player()
    {
        CurrentTexture = Assets.GetTexture("player");
    }
    
    public override void Update(float dt)
    {
        bool left = InputManager.GetKeyDown(Keys.Left) || InputManager.GetButtonDown(Buttons.LeftThumbstickLeft);
        bool right = InputManager.GetKeyDown(Keys.Right) || InputManager.GetButtonDown(Buttons.LeftThumbstickRight);
        bool jump = InputManager.GetKeyDown(Keys.Space) || InputManager.GetButtonDown(Buttons.A);

        if (left)
            _velocity.X = -5f;
        if (right)
            _velocity.X = 5f;
        if (left == right)
            _velocity.X = 0;

        _velocity.Y += 0.25f;

        if (jump)
            _velocity.Y = -10f;

        Position += _velocity;
    }
}
