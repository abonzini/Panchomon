using Gameplay.GameplayElements;

namespace Gameplay.GameEngine
{
    /// <summary>
    /// The type of things that are actually phisically located on a tile, one per tile
    /// </summary>
    public class PhysicalInstance : GameplayElement
    {
        public Tile AssociatedTile; /// Contains coordinates of object to see where the object is
        public virtual char DebugChar { get; set; }
        public virtual bool IsSolid { get; set; } = true; // Most objects are solid so units will not be able to plan through it
        // Fields that are reset or calculated during battle
        public InstanceStateMachine Sm; // Its sm
        // Methods
        public PhysicalInstance Clone()
        {
            return (PhysicalInstance)MemberwiseClone();
        }
    }
}
