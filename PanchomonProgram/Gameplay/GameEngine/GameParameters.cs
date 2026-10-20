using Gameplay.GameplayElements;

namespace Gameplay.GameEngine
{
    public class GameParameters
    {
        public TerrainGenerator TerrainGenerator; /// The generator that generates the corresponding desired field
        public FieldModifier FieldModifier = null; /// The field mod in the battlefield (will change gen or add weathers)
        public Side FocusedSide = Side.ALL; /// For single-player adventures. Aborts the match if this side loses because no one'll care how it continues
    }
}
