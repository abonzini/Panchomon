using Gameplay.GameplayElements;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Gameplay.GameEngine
{
    /// <summary>
    /// Whtether mon can participate in battle
    /// </summary>
    public enum BattleAvailability
    {
        READY,
        BUSY,
        FAINT,
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AdditionalPokemonParameter
    {
        BOSS, /// Mon is a boss and the whole team loses if boss dies
        GIANT, /// Boss is giant which means it's just like... very big and bigger hitbox and stuff
    }
    /// <summary>
    /// An instance of a pokemon in the simulator
    /// </summary>
    public class PokemonInstance : PhysicalInstance
    {
        public PokemonEntity AssociatedEntity = null;
        public double HpPercent = 1;
        public List<NonVolatileStatus> NonVolatileStatusConditions = [];
        public HashSet<AdditionalPokemonParameter> AdditionalParameters = [];
        public BattleAvailability Availability = BattleAvailability.READY;
        public double[] BaseStatMult = [1, 1, 1, 1, 1, 1];
        public double[] VolatileStatMult = [1, 1, 1, 1, 1, 1];
        public override string ToString()
        {
            return $"{AssociatedEntity} ({HpPercent * 100}%)";
        }
        public override string Name { get => AssociatedEntity.Name; set => base.Name = value; } // Use associated entitiy's name

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
