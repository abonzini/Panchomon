using Gameplay.GameplayElements;

namespace Gameplay.GameEngine
{
    /// <summary>
    /// The type of things that are actually phisically located on a tile, one per tile
    /// </summary>
    public class PhysicalInstance : SimulationElement
    {
        public Tile AssociatedTile; /// Contains coordinates of object to see where the object is
        public PhysicalInstance Clone()
        {
            return (PhysicalInstance)MemberwiseClone();
        }
    }
}
