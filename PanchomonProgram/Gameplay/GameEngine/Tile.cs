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
        public Surface Surface = null;
        public GroundEffect GroundEffect = null;
        public PhysicalInstance Occupier = null;
    }
}
