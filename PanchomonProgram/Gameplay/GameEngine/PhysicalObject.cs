namespace Gameplay.GameEngine
{
    /// <summary>
    /// Stuff that goes on a tile or in the floor of a dungeon, may or may not have additional interactions
    /// </summary>
    public class PhysicalObject : PhysicalInstance
    {
        static public IEnumerable<PhysicalInstance> GetAllOccupierObjects()
        {
            Console.WriteLine("Getting all Physical Objects");
            yield break;
        }
    }
}
