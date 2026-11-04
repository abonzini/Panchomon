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
            yield return new PhysicalObject()
            {
                Name = "Dark Painted Wall",
                _description = "Wall to be used for separating rooms (modern setting).",
                DebugChar = '█'
            };
            yield return new PhysicalObject()
            {
                Name = "Paradox Table",
                _description = "Table seen in Paradox venue.",
                DebugChar = 'P'
            };
            yield return new PhysicalObject()
            {
                Name = "Paradox Chair",
                _description = "Chair seen in Paradox venue.",
                DebugChar = 'p'
            };
            yield return new PhysicalObject()
            {
                Name = "Short Plant",
                _description = "Small plant/fern in an office.",
                DebugChar = 'y'
            };
            yield return new PhysicalObject()
            {
                Name = "Piled Boxes",
                _description = "Pile of boxes in japanese workspace.",
                DebugChar = '▢'
            };
            yield return new PhysicalObject()
            {
                Name = "Piled Books",
                _description = "Pile of books in the floor.",
                DebugChar = '≡'
            };
            yield return new PhysicalObject()
            {
                Name = "Bookshelf",
                _description = "Bookshelf.",
                DebugChar = '▤'
            };
            yield return new PhysicalObject()
            {
                Name = "Computer Desk",
                _description = "A computer desk with PC.",
                DebugChar = '▣'
            };
            yield return new PhysicalObject()
            {
                Name = "Office Chair",
                _description = "An office chair to be paired with a pc.",
                DebugChar = '▪'
            };
            yield return new PhysicalObject()
            {
                Name = "Lab Tank",
                _description = "Futuristic looking lab tank.",
                DebugChar = 'Ü'
            };
        }
    }
}
