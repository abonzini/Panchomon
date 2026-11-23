using Gameplay.GameEngine;

namespace GameSimulator
{
    // This part is the API of the battle sim, hooks that do stuff that alter the battle state in an event stack. Moves, abilities, etc will enter from here when allowed in a potentially controlled manner
    public partial class GameSimulator : IGameSimulatorApi
    {
    }
}
