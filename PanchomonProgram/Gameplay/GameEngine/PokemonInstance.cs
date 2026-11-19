using Gameplay.GameplayElements;

namespace Gameplay.GameEngine
{
    /// <summary>
    /// An instance of a pokemon in the simulator
    /// </summary>
    public class PokemonInstance : PhysicalInstance
    {
        public PokemonEntity AssociatedEntity = null;
        public double HpPercent = 1;
        public List<StatusCondition> StatusConditions = [];
        public bool IsBoss = false;
        public double[] BaseStatMult = [1, 1, 1, 1, 1, 1];
        public override string ToString()
        {
            return $"{AssociatedEntity} ({HpPercent * 100}%)";
        }
        public override string Name { get => AssociatedEntity.Name; set => base.Name = value; } // Use associated entity's name
        public override char DebugChar { get => AssociatedEntity.Name[0]; set => base.DebugChar = value; } // Use associated entitiy's name first char

        /// <summary>
        /// Used to create a pokemon gameplay instance from a base Pokemon. Deals with defining the remaining sets (move disks) as well as set up Hp and etc. Can then be made permanent for the rest of the event
        /// </summary>
        /// <param name="pokemon">Pokemon to instance</param>
        /// <param name="rng">The Rng to define the rest of this mon's things</param>
        /// <returns>The generated Pokemon instance ready for game</returns>
        public PokemonInstance(PokemonEntity pokemon, Random rng)
        {
            AssociatedEntity = pokemon;
        }
    }
}
