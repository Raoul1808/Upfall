using Brocco;
using Microsoft.Xna.Framework;

namespace Upfall.Entities;

public abstract class TilemapEntity : Entity
{
    public virtual void OnTileTopTouched(Rectangle tile)
    {
    }
    
    public virtual void OnTileBottomTouched(Rectangle tile)
    {
    }
    
    public virtual void OnTileLeftTouched(Rectangle tile)
    {
    }
    
    public virtual void OnTileRightTouched(Rectangle tile)
    {
    }

    public virtual void Kill()
    {
    }
}
