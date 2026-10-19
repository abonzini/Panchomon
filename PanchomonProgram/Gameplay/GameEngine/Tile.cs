using Gameplay.GameplayElements;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Gameplay.GameEngine
{
    /// <summary>
    /// How the cliff face looks like
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CliffFace
    {
        NONE, // Non-rendered
        STONE
    }
    public class Tile : SimulationElement
    {
        public CliffFace CliffFace = CliffFace.NONE;
        public PhysicalElement Occupier = null;
        public Surface Surface = null;
        public int Elevation = 0; // Elevation in Pixels w.r.t. ground level, usually for sunken tiles like water but ig can be animated?
        public GroundEffect GroundEffect = null;
    }
}
