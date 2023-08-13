using Brocco;
using Microsoft.Xna.Framework;

namespace Upfall.Entities;

public class Platform : Entity
{
    public Platform()
    {
        CurrentTexture = Assets.Pixel;
        Color = Color.Black;
        Position = new Vector2(50, 300);
        Scale = new Vector2(400, 50);
        Anchor = Anchor.MiddleLeft;
    }
    
    public override void Update(float dt)
    {
    }
}
