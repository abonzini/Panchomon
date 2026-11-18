namespace Gameplay.GameplayElements
{
    public class PokeBall : SimulationElement
    {
        // Properties
        public PokemonType CramType = PokemonType.NONE;
        // Massive list of all of them and their effects (less friendly than storing in Json but allow for complex behaviours)
        public static IEnumerable<PokeBall> GetAllPokeballs()
        {
            Console.WriteLine("Getting all PokeBalls");
            yield return new PokeBall()
            {
                Name = "Poke Ball",
                CramType = PokemonType.NORMAL,
                _description = "Allows the capture of any Pokemon of Rank 1 and Below"
            };
            yield return new PokeBall()
            {
                Name = "Great Ball",
                CramType = PokemonType.NORMAL,
                _description = "Allows the capture of any Pokemon of Rank 2 and Below"
            };
            yield return new PokeBall()
            {
                Name = "Ultra Ball",
                CramType = PokemonType.NORMAL,
                _description = "Allows the capture of any Pokemon of Rank 3 and Below"
            };
        }
    }
}
