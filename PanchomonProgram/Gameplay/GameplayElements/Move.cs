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
    public class Move : GameplayElement
    {
        // Properties
        public PokemonType Type = PokemonType.NONE;
        public MoveCategory Category = MoveCategory.STATUS;
        public int Bars = 3; // How many charging bars it takes
        public int Bp = 0; // Only for damaging moves
        public DamageType DamageType = DamageType.DAMAGE_FORMULA;
        // Later on, targeting, execution type, etc
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
                _description = "User skips their turn"
            };
            yield return new Move()
            {
                Name = "Return",
                Type = PokemonType.NORMAL,
                Bp = 102,
                Category = MoveCategory.PHYSICAL,
                DamageType = DamageType.DAMAGE_FORMULA,
                Bars = 3
            };
        }
    }
}
