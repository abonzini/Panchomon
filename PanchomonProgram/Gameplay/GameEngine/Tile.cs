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
        STONE,
        WATERFALL,
        TILE
    }
    public class Tile : GameplayElement
    {
        public CliffFace CliffFace { get; set; } = CliffFace.NONE;
        public Surface Surface = null;
        public TileEffect Effect = null;
        public PhysicalInstance Occupier = null;
        public readonly int X; /// Its coords. Immutable
        public readonly int Y;
        public Tile(int x, int y)
        {
            X = x;
            Y = y;
        }
        /// <summary>
        /// Whether this surface can hinder movement, important when map generating in order to not put tiles that block the way passively or actively
        /// </summary>
        /// <returns>Whether this tile will alter movement to entities moving through it</returns>
        public bool IsMovementAffected()
        {
            return !(Surface.HindersMovement || (Occupier != null && Occupier.IsSolid)); // Tile not walkable depending on the surface or if it has an occupier
        }
        public override string ToString()
        {
            return $"{Surface?.Name}";
        }
    }
}
