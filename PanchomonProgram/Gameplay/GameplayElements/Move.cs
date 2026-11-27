using System.Text;
using Utilities;

namespace Gameplay.GameplayElements
{
    public enum MoveCategory
    {
        STATUS,
        PHYSICAL,
        SPECIAL
    }
    public enum DamageType // How damage is calculated for a move
    {
        DAMAGE_FORMULA, // Damage formula is the classic pokemon-inspired one, (BP * A * mults)/ (2 * D)
        EXACT, // Exact damage to HP, replaces the damage formula and mults and subtracts directly from HP
        PROPORTIONAL // A proportion of HP before mult in percent (e.g. nature madness doing 50%, formula is Hp * Bp / 100)
    }
    public enum TargetingMode
    {
        NONE, // Move doesn't target, can be used immediately
        TILES, // Move targets tiles in front and side of the caster, can be many tiles, while they all apply simultaneously. Direction is always the caster's no matter the tile
        STRAIGHT_LINE, // Move targets a line projected from a start tile relative in orientation to front of caster
        RANGED_LINE, // Move targets a line but which is free to move within a range and not limited to the 8 cardinal directions
    }
    /// <summary>
    /// Defines how a move is targeted, the amount of estimated bp when this mode is used
    /// </summary>
    public class MoveTargetingMode // The default is the most common type, a forward instant tile
    {
        public TargetingMode Mode = TargetingMode.NONE;
        public int MinRange = 0; // These only apply for the targeting modes that don't involve absolute positions (i.e lines)
        public int MaxRange = 0;
        public List<(int, int)> TilesAffected; // This is relative to forward, backwards, etc relative to where unit is facing, first forward then lateral with + being right of
        public List<int> Bpmod; // How much of the Bp this tile applies, in order to calculate (max) damage
        public bool TargetsEnemy = true; // Will get the target from here, either enemy or ally (moves that may hit both will have different target modes to choose properly)
        // Space for additional conditions if needed
    }
    public class Move : GameplayElement
    {
        // Statics for common move definitions
        static readonly MoveTargetingMode _noTargetMode = new MoveTargetingMode(); // Nothing targeted, nothing to do
        static readonly MoveTargetingMode _basicTargetingMode = new MoveTargetingMode() // This is the basic kind where mon just hits forward
        {
            Mode = TargetingMode.TILES,
            TilesAffected = [(1, 0)],
            Bpmod = [1],
            TargetsEnemy = true
        };
        // Properties
        public PokemonType Type = PokemonType.NONE;
        public MoveCategory Category = MoveCategory.STATUS;
        public int Bars = 3; // How many charging bars it takes
        public int Bp = 0; // Only for damaging moves
        public DamageType DamageType = DamageType.DAMAGE_FORMULA;
        public List<MoveTargetingMode> TargetingModes; // May be more than once, in which case there may be alternative ways to understand the move, will need to implement other conditions here
        // Later on, execution hooks, etc
        public override string GetDescription()
        {
            StringBuilder fullString = new StringBuilder();
            fullString.Append($"{GeneralUtilities.ApaCapitalize(Type.ToString())}-type move ({GeneralUtilities.ApaCapitalize(Category.ToString())}) {new string('■', Bars)}. ");
            if (Category != MoveCategory.STATUS)
            {
                string damageTypeString = DamageType switch
                {
                    DamageType.DAMAGE_FORMULA => $"BP: {Bp}. ",
                    DamageType.EXACT => $"Deals Exactly {Bp} damage. ",
                    DamageType.PROPORTIONAL => $"Deals {Bp}% of opponent's HP. ",
                    _ => throw new Exception("Unrecognized damage type")
                };
                fullString.Append(damageTypeString);
            }
            fullString.Append(_description);
            return fullString.ToString();
        }
        // Massive list of all of them and their effects (less friendly than storing in Json but allow for complex behaviours)
        public static IEnumerable<Move> GetAllMoves()
        {
            Console.WriteLine("Getting all Moves");
            yield return new Move()
            {
                Name = "Idle",
                Category = MoveCategory.STATUS,
                Type = PokemonType.NORMAL,
                _description = "User skips their turn",
                TargetingModes = [_noTargetMode]
            };
            yield return new Move()
            {
                Name = "Return",
                Type = PokemonType.NORMAL,
                Bp = 102,
                Category = MoveCategory.PHYSICAL,
                DamageType = DamageType.DAMAGE_FORMULA,
                Bars = 3,
                TargetingModes = [_basicTargetingMode]
            };
        }
    }
}
