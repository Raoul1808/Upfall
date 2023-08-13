using Brocco;
using Brocco.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Upfall.Entities;

public class Player : Entity
{
    private const float MaxFallSpeed = 40f;
    private const float Gravity = 20f;
    private const float HorizontalSpeed = 5f;
    private const float JumpForce = 7f;
    
    public Player()
    {
        CurrentTexture = Assets.GetTexture("player");
        Position = new Vector2(100, 100);
    }
    
    public override void Update(float dt)
    {
        bool left = InputManager.GetKeyDown(Keys.Left) || InputManager.GetButtonDown(Buttons.LeftThumbstickLeft);
        bool right = InputManager.GetKeyDown(Keys.Right) || InputManager.GetButtonDown(Buttons.LeftThumbstickRight);
        bool jump = InputManager.GetKeyPress(Keys.Space) || InputManager.GetButtonPress(Buttons.A);

        if (left)
            Velocity.X = -HorizontalSpeed;
        if (right)
            Velocity.X = HorizontalSpeed;
        if (left == right)
            Velocity.X = 0;

        Velocity.Y += Gravity * dt;
        if (Velocity.Y >= MaxFallSpeed)
            Velocity.Y = MaxFallSpeed;

        if (jump)
            Velocity.Y = -JumpForce;
    }

    public void ResolveTileCollision(Rectangle other)
    {
        float halfWidth = BoundingBox.Width / 2f;
        float halfHeight = BoundingBox.Height / 2f;
        
        if (this.TouchedLeftOf(other))
        {
            Position.X = other.Left - halfWidth;
            Velocity.X = 0;
        }
        if (this.TouchedRightOf(other))
        {
            Position.X = other.Right + halfWidth;
            Velocity.X = 0;
        }
        if (this.TouchedTopOf(other))
        {
            Position.Y = other.Top - halfHeight;
            Velocity.Y = 0;
        }
        if (this.TouchedBottomOf(other))
        {
            Position.Y = other.Bottom + halfHeight;
            Velocity.Y = 0;
        }
    }
}
