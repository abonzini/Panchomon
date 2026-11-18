using Utilities;

namespace Gameplay.GameplayElements
{
    /// <summary>
    /// All gameplay elements with a name a description
    /// </summary>
    public class GameplayElement
    {
        public virtual string Name { get; set; } = ""; // This means the name could technically be overriden
        protected string _description = "";
        public virtual string GetDescription()
        {
            return _description;
        }
        public override string ToString()
        {
            return Name;
        }
        /// <summary>
        /// Gets the generic name of an item like Fire Gem
        /// </summary>
        /// <param name="type">Type in name</param>
        /// <param name="name">Name of item</param>
        /// <returns></returns>
        public static string GetName(PokemonType type, string name)
        {
            return $"{GeneralUtilities.ApaCapitalize(type.ToString())} {name}";
        }
    }
    /// <summary>
    /// A gameplay element but with battle effects, e.g. they can declare event hooks for special properties
    /// </summary>
    public class SimulationElement : GameplayElement
    {
        // The hooks, e.g. :
        //public Func<Move, float> OnGetDamageModifier { get; set; } = delegate (Move move) { return 1; };
        // Use as: OnGetDamageModifier = delegate (Move move) { return 2; } when defining new instances
    }
}
