namespace Gameplay.GameEngine
{
    /// <summary>
    /// A brush defines what a tile will be painted as, including Surface, Grounded Effect or Physical Thing on top
    /// They're all strings to be able to be fetched from their corresponding places. A null string implies that the brush is not applied, while an empty string works as an eraser, actively removing stuff
    /// </summary>
    public class TileBrush
    {
        public string CliffFace { get; set; } = null;
        public string TileSurface { get; set; } = null;
        public string TileEffect { get; set; } = null;
        public string Occupier { get; set; } = null;
        public int Size { get { return string.IsNullOrEmpty(Occupier) ? 1 : GameplayElementsContainer.GameplayElementsContainer.GlobalData.PhysicalObjects[Occupier].Size; } }
    }
    /// <summary>
    /// Contains all the elements for terrain generation, this includes all the elements that will result in a completed map (although later on the weather mods will alter these)
    /// </summary>
    public class TerrainGenerator
    {
        public TileBrush BasicBrush { get; set; }
        public List<TilePropagatorAction> PropagatorActions { get; set; } = null; /// Propagators (in order of appearance) to generate terrain procedurally. Initially will target all the proper battleSides but afterwards can continue iterating
    }
}
