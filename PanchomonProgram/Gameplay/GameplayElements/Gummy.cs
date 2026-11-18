using Utilities;

namespace Gameplay.GameplayElements
{
    public class Gummy : GameplayElement
    {
        public PokemonType Type = PokemonType.NONE;
        public override string ToString()
        {
            return Name;
        }
        public static string GetGummyOfType(PokemonType type)
        {
            string berryName = type switch
            {
                PokemonType.NORMAL => "White",
                PokemonType.FIGHTING => "Orange",
                PokemonType.FLYING => "Sky",
                PokemonType.POISON => "Pink",
                PokemonType.GROUND => "Brown",
                PokemonType.ROCK => "Gray",
                PokemonType.BUG => "Green",
                PokemonType.GHOST => "Purple",
                PokemonType.STEEL => "Silver",
                PokemonType.FIRE => "Red",
                PokemonType.WATER => "Blue",
                PokemonType.GRASS => "Grass",
                PokemonType.ELECTRIC => "Yellow",
                PokemonType.PSYCHIC => "Gold",
                PokemonType.ICE => "Clear",
                PokemonType.DRAGON => "Royal",
                PokemonType.DARK => "Black",
                PokemonType.FAIRY => "Rainbow",
                _ => throw new Exception("Invalid pokemon type")
            };
            berryName += " Berry";
            return berryName;
        }
        public static IEnumerable<Gummy> GetAllGummies()
        {
            Console.WriteLine("Getting all Gummies");
            foreach (PokemonType type in Enum.GetValues<PokemonType>())
            {
                if (type == PokemonType.NONE) continue;
                yield return new Gummy()
                {
                    Name = GetGummyOfType(type),
                    Type = type,
                    _description = $"A {GeneralUtilities.ApaCapitalize(type.ToString())}-type gummy."
                };
            }
        }
    }
}