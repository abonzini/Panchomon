using Gameplay.GameplayElements;

namespace Gameplay.GameEngine
{
    /// <summary>
    /// The type of things that are actually phisically located on a tile, one per tile
    /// </summary>
    public class PhysicalInstance : SimulationElement
    {
        public int Size = 1; /// The size of all things. Things can only be square since we don't want to deal with rotations
        public int X; /// Contains coordinates of object to see where the object is (centred)
        public int Y; /// Contains coordinates of object to see where the object is (centred)
        public PhysicalInstance Clone()
        {
            return (PhysicalInstance)MemberwiseClone();
        }
        static public IEnumerable<PhysicalInstance> GetAllOccupierObjects()
        {
            Console.WriteLine("Getting all Physical Objects");
            yield break;
        }
    }
}
