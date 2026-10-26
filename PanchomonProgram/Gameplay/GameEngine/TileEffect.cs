using Gameplay.GameplayElements;

namespace Gameplay.GameEngine
{
    /// <summary>
    /// Tile effect is something located on a tile that doesn't alter movement but has effects when stepped on, like Sticky Web
    /// </summary>
    public class TileEffect : SimulationElement
    {
        public Tile AssociatedTile; // To be able to destory itself or something
        public TileEffect Clone()
        {
            return (TileEffect)MemberwiseClone();
        }
        static public IEnumerable<TileEffect> GetAllTileEffects()
        {
            Console.WriteLine("Getting all Tile Effects");
            yield break;
        }
    }
}
