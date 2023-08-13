using System;
using Brocco;
using Brocco.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Upfall.Entities;

internal class Player : Entity
{
    public Player()
    {
        CurrentTexture = Assets.GetTexture("ok");
    }
    
    public override void Update(float dt)
    {
        Vector2 vel = Vector2.Zero;
        
        if (InputManager.GetKeyDown(Keys.Z))
            vel.Y--;
        if (InputManager.GetKeyDown(Keys.S))
            vel.Y++;
        if (InputManager.GetKeyDown(Keys.Q))
            vel.X--;
        if (InputManager.GetKeyDown(Keys.D))
            vel.X++;
        
        if (vel != Vector2.Zero)
            vel.Normalize();
        Position += vel;

        Console.WriteLine("I am at " + Position);
    }
}
