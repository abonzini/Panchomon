using Gameplay.GameplayElements;
using Utilities;

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
        public List<Move> ActiveMoves = [];
        public List<Ability> ActiveAbilities = new List<Ability>();
        public HeldItem ActiveHeldItem = null;
        public override string ToString()
        {
            return $"{AssociatedEntity} ({HpPercent * 100}%)";
        }
        public override string Name { get => AssociatedEntity.Name; set => base.Name = value; } // Use associated entity's name
        public override char DebugChar { get => AssociatedEntity.Name[0]; set => base.DebugChar = value; } // Use associated entitiy's name first char

        /// <summary>
        /// Used to create a pokemon gameplay instance from a base Pokemon. Deals with defining the remaining sets (move disks) as well as set up Hp and etc. Can then be made permanent for the rest of the event
        /// </summary>
        /// <param name="pokemonEntity">Pokemon to instance</param>
        /// <param name="rng">The Rng to define the rest of this mon's things</param>
        /// <returns>The generated Pokemon instance ready for game</returns>
        public PokemonInstance(PokemonEntity pokemonEntity, Random rng)
        {
            AssociatedEntity = pokemonEntity;
            // Init fields
            ActiveHeldItem = AssociatedEntity.HeldItem;
            for (int i = 0; i < AssociatedEntity.Abilities.Length; i++) // Add the active abilities
            {
                if (AssociatedEntity.Abilities[i] != null && AssociatedEntity.AbilityActive[i])
                {
                    ActiveAbilities.Add(AssociatedEntity.Abilities[i]);
                }
            }
            // Add moves
            for (int i = 0; i < AssociatedEntity.Moves.Length; i++)
            {
                if (AssociatedEntity.Moves[i] != null)
                {
                    ActiveMoves.Add(AssociatedEntity.Moves[i]);
                }
            }
            // Finally roll the move disks
            for (int i = 0; i < AssociatedEntity.MoveDisks.Length; i++)
            {
                MoveDisk disk = AssociatedEntity.MoveDisks[i];
                if (disk != null) // Found disk, get moves
                {
                    Move theMove;
                    if (disk.IsRandomMove)
                    {
                        List<Move> diskMovePool = disk.Type switch
                        {
                            PokemonType.NONE => [.. pokemonEntity.Species.Moveset.Except(ActiveMoves)], // Blank disk! Then just get a moveset one
                            _ => [.. pokemonEntity.Species.Moveset.Except(ActiveMoves).Where(m => m.Type == disk.Type)] // Type disk, get move of such type
                        };
                        theMove = GeneralUtilities.GetRandomPick(diskMovePool, rng); // Finally, choose a random one
                    }
                    else
                    {
                        theMove = disk.AddedMove;
                    }
                    if (theMove != null)
                    {
                        ActiveMoves.Add(theMove);
                    }
                }
            }
            // And finally finally, fill with idling
            Move idle = GameplayElementsContainer.GameplayElementsContainer.GlobalData.Moves["Idle"];
            while (ActiveMoves.Count < GameplayElementsContainer.GameplayElementsContainer.NUM_BATTLE_MOVES) // Fill the remaining (4) with idle
            {
                ActiveMoves.Add(idle);
            }
        }
    }
}
