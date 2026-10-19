namespace Gameplay.GameEngine
{
    /// <summary>
    /// How the cliff face looks like
    /// </summary>
    public enum CliffFace
    {
        NONE, // Non-rendered
        STONE
    }
    public class Tile
    {
        public CliffFace CliffFace = CliffFace.NONE;
        public PhysicalElement Occupier = null;
        public Surface Surface = null;
        public int Elevation = 0; // Elevation in Pixels w.r.t. ground level, usually for sunken tiles like water but ig can be animated?
        public GroundEffect GroundEffect = null;
    }
}
