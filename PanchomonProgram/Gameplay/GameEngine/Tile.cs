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
        public CliffFace CliffFace { get; set; } = CliffFace.NONE;
        public Surface Surface = null;
        public TileEffect Effect = null;
        public PhysicalInstance Occupier = null;
        public bool IsWalkable()
        {
            return !(Surface.AltersMovement || Occupier != null); // Tile not walkable depending on the surface or if it has an occupier
        }
        public override string ToString()
        {
            return $"{Surface?.Name}";
        }
    }
}
