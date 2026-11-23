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
}
