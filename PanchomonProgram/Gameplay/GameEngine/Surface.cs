using Gameplay.GameplayElements;

namespace Gameplay.GameEngine
{
    /// <summary>
    /// What a tile is made of, will do stuff depending on who enters, who leaves, etc
    /// </summary>
    public class Surface : SimulationElement
    {
        /// <summary>
        /// Get all so they can be fetched by name
        /// </summary>
        /// <returns>Surfaces, one by one</returns>
        public static IEnumerable<Surface> GetAllSurfaces()
        {
            Console.WriteLine("Getting all Surfaces");
            yield return new Surface()
            {
                Name = "None",
                _description = "Placeholder for when a surface doesn't exist."
            };
        }
    }
}
