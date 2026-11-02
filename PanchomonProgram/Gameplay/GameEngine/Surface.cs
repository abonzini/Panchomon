using Gameplay.GameplayElements;

namespace Gameplay.GameEngine
{
    /// <summary>
    /// What a tile is made of, will do stuff depending on who enters, who leaves, etc
    /// </summary>
    public class Surface : SimulationElement
    {
        public int Elevation = 0; // Elevation of this tile in pixels (to add some fluidity to the pixel art)
        public bool HindersMovement = false; // Defines whether this surface impedes or is harmful to _some_ mons, in order to be treated as a wall
        public ConsoleColor DebugColor = ConsoleColor.White;
        /// <summary>
        /// Get all so they can be fetched by name
        /// </summary>
        /// <returns>Surfaces, one by one</returns>

        public static IEnumerable<Surface> GetAllSurfaces()
        {
            Console.WriteLine("Getting all Surfaces");
            yield return new Surface()
            {
                Name = "Grass",
                _description = "Grassy stuff.",
                HindersMovement = false,
                DebugColor = ConsoleColor.Green
            };
            yield return new Surface()
            {
                Name = "Dirt",
                _description = "Dirt as commonly seen in reg Pokemon battles.",
                HindersMovement = false,
                DebugColor = ConsoleColor.Yellow
            };
            yield return new Surface()
            {
                Name = "White Paint",
                _description = "White Paint markings, normally used to delimit regulatory boundaries.",
                HindersMovement = false,
                DebugColor = ConsoleColor.White
            };
            yield return new Surface()
            {
                Name = "Water",
                _description = "Standard water tile.",
                HindersMovement = true,
                DebugColor = ConsoleColor.Cyan
            };
            yield return new Surface()
            {
                Name = "Conductive Cables",
                _description = "Grid-like cables on the floor.",
                HindersMovement = false,
                DebugColor = ConsoleColor.DarkGray
            };
            yield return new Surface()
            {
                Name = "Paradox Tile",
                _description = "Paradox tile floor used for the paradox venue.",
                HindersMovement = false,
                DebugColor = ConsoleColor.DarkMagenta
            };
            yield return new Surface()
            {
                Name = "Clean Tile",
                _description = "Clean Tile as seen in laboratory settings.",
                HindersMovement = false,
                DebugColor = ConsoleColor.Gray
            };
        }
    }
}
