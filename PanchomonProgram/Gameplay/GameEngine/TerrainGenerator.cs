namespace Gameplay.GameEngine
{
    /// <summary>
    /// Contains all the elements for terrain generation, this includes all the elements that will result in a completed map (although later on the weather mods will alter these)
    /// </summary>
    public class TerrainGenerator
    {
        public string BasicTileSurface { get; set; } = "";
        public string FillerTile { get; set; } = ""; /// To be used for "filler" e.g. for tiles that are technically outside of the main board but are still playable
        public CliffFace CliffFace { get; set; } = CliffFace.NONE;
        public List<TilePropagator> Propagators { get; set; } = []; /// Propagators (in order of appearance) to generate terrain procedurally 
    }
}
