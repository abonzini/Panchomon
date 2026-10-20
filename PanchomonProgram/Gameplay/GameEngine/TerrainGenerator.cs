namespace Gameplay.GameEngine
{
    /// <summary>
    /// Contains all the elements for terrain generation, this includes all the elements that will result in a completed map (although later on the weather mods will alter these)
    /// </summary>
    public class TerrainGenerator
    {
        public string BasicTileSurface { get; set; } = "";
        public CliffFace CliffFace { get; set; } = CliffFace.NONE; /// How the cliff face will look
        public List<TilePropagatorAction> PropagatorActions { get; set; } = []; /// Propagators (in order of appearance) to generate terrain procedurally 
    }
}
