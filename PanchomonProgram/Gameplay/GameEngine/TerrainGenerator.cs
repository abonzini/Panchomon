namespace Gameplay.GameEngine
{
    /// <summary>
    /// A brush defines what a tile will be painted as, including Surface, Grounded Effect or Physical Thing on top
    /// They're all strings to be able to be fetched from their corresponding places. A null string implies that the brush is not applied, while an empty string works as an eraser, actively removing stuff
    /// </summary>
    public class TileBrush
    {
        public string TileSurface { get; set; } = null;
        public string GroundEffectSurface { get; set; } = null;
        public string ObjectSurface { get; set; } = null;
        // Hash based equality, to quick compare brushes
        private int? _brushHash;
        public override int GetHashCode()
        {
            if (!_brushHash.HasValue)
            {
                HashCode newHash = new HashCode();
                newHash.Add(TileSurface);
                newHash.Add(GroundEffectSurface);
                newHash.Add(ObjectSurface);
                _brushHash = newHash.ToHashCode();
            }
            return _brushHash.Value;
        }
    }
    /// <summary>
    /// Contains all the elements for terrain generation, this includes all the elements that will result in a completed map (although later on the weather mods will alter these)
    /// </summary>
    public class TerrainGenerator
    {
        public TileBrush BasicBrush { get; set; }
        public CliffFace CliffFace { get; set; } = CliffFace.NONE; /// How the cliff face will look
        public List<TilePropagatorAction> PropagatorActions { get; set; } = null; /// Propagators (in order of appearance) to generate terrain procedurally. Initially will target all the proper battleSides but afterwards can continue iterating
    }
}
