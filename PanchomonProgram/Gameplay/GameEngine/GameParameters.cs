using Gameplay.GameplayElements;

namespace Gameplay.GameEngine
{
    public class GameParameters
    {
        public List<Weather> Weathers = []; /// The weathers in the battlefield (or empty lists)
        public Side FocusedSide = Side.NONE; /// For single-player adventures. Aborts the match if this side loses because no one'll care how it continues
    }
}
