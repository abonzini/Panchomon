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
                Name = "Stool",
                _description = "Chair seen in Paradox venue.",
                DebugChar = ','
            };
            yield return new PhysicalObject()
            {
                Name = "Short Plant",
                _description = "Small plant/fern.",
                DebugChar = 'y'
            };
            yield return new PhysicalObject()
            {
                Name = "Tall Plant",
                _description = "Tall plant.",
                DebugChar = 'Y'
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
                Name = "Computer Desk (W)",
                _description = "Inverted computer to do back to back computer desks.",
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
            yield return new PhysicalObject()
            {
                Name = "Table",
                _description = "Table for dinner.",
                DebugChar = '╤'
            };
            yield return new PhysicalObject()
            {
                Name = "Tv",
                _description = "Tv.",
                DebugChar = '▣'
            };
            yield return new PhysicalObject()
            {
                Name = "Whiteboard",
                _description = "A classroom whiteboard.",
                DebugChar = '▰'
            };
            yield return new PhysicalObject()
            {
                Name = "Student Desk",
                _description = "Desk + Chair combo for students.",
                DebugChar = '╤'
            };
            yield return new PhysicalObject()
            {
                Name = "Trash Can",
                _description = "Typical Pokemon trash can.",
                DebugChar = '◎'
            };
            yield return new PhysicalObject()
            {
                Name = "Power Plant Barrel",
                _description = "Barrel found in power plant inspired by FRLG power plant's barrel.",
                DebugChar = '◉'
            };
            yield return new PhysicalObject()
            {
                Name = "Electric Panel",
                _description = "Decorative electric panel for APP walls in certain rooms.",
                DebugChar = '▥'
            };
            yield return new PhysicalObject()
            {
                Name = "Lighting Rod",
                _description = "Lighting rod that contains lighting and redirects.",
                DebugChar = 'A'
            };
            yield return new PhysicalObject()
            {
                Name = "Big Generator (L)",
                _description = "Big generator for boss room, left side.",
                DebugChar = '╭'
            };
            yield return new PhysicalObject()
            {
                Name = "Big Generator (R)",
                _description = "Big generator for boss room, right side.",
                DebugChar = '╮'
            };
            yield return new PhysicalObject()
            {
                Name = "Big Generator (C)",
                _description = "Big generator for boss room, center side.",
                DebugChar = '─'
            };
            yield return new PhysicalObject()
            {
                Name = "Small Generator",
                _description = "Small generator, same effect but only 1 tile.",
                DebugChar = '▟'
            };
        }
    }
}
