using Gameplay.GameplayElements;

namespace Gameplay.GameEngine
{
    /// <summary>
    /// What a tile is made of, will do stuff depending on who enters, who leaves, etc
    /// </summary>
    public class Surface : SimulationElement
    {
        public int Elevation = 0; // Elevation of this tile in pixels (to add some fluidity to the pixel art)
        public bool AltersMovement = false; // Defines whether this surface impedes or is harmful to _some_ mons, in order to be treated as a wall
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
                Name = "Dirt",
                _description = "Dirt as commonly seen in reg Pokemon battles.",
                AltersMovement = false,
                DebugColor = ConsoleColor.Yellow
            };
            yield return new Surface()
            {
                Name = "White Paint",
                _description = "White Paint markings, normally used to delimit regulatory boundaries.",
                AltersMovement = false,
                DebugColor = ConsoleColor.White
            };
            yield return new Surface()
            {
                Name = "Water",
                _description = "Standard water tile.",
                AltersMovement = true,
                DebugColor = ConsoleColor.Cyan
            };
            yield return new Surface()
            {
                Name = "Conductive Cables",
                _description = "Grid-like cables on the floor.",
                AltersMovement = false,
                DebugColor = ConsoleColor.Gray
            };
        }
    }
}
