namespace Gameplay.GameEngine
{
    public class GameParameters
    {
        public TerrainGenerator TerrainGenerator; /// The generator that generates the corresponding desired field
        public List<Side> SidesPresent = [];
        public Side FocusedSide = Side.ALL; /// For single-player adventures. Aborts the match if this side loses because no one'll care how it continues
    }
}
