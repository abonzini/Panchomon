using Gameplay.GameplayElements;

namespace Gameplay.GameEngine
{
    /// <summary>
    /// What a tile is made of, will do stuff depending on who enters, who leaves, etc
    /// </summary>
    public class Surface : SimulationElement
    {
        public int Elevation = 0; /// Elevation of this tile in pixels (to add some fluidity to the pixel art)
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
                _description = "Dirt as commonly seen in reg Pokemon battles."
            };
            yield return new Surface()
            {
                Name = "White Paint",
                _description = "White Paint markings, normally used to delimit regulatory boudnaries."
            };
            yield return new Surface()
            {
                Name = "Water",
                _description = "Standard water tile."
            };
            yield return new Surface()
            {
                Name = "Conductive Cables",
                _description = "Grid-like cables on the floor."
            };
        }
    }
}
