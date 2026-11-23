namespace Gameplay.GameplayElements
{
    public class Mint : GameplayElement
    {
        public Nature AssociatedNature = null;
        public override string GetDescription()
        {
            string description = $"Permanently changes a Pokemon's nature to {AssociatedNature.Name}: ";
            description += AssociatedNature.GetDescription();
            return description;
        }
    }
    public class Nature : GameplayElement
    {
        // Massive list of all of them and their effects (less friendly than storing in Json but allow for complex behaviours)
        public static IEnumerable<Nature> GetAllNatures()
        {
            Console.WriteLine("Getting all Natures");
            yield return new Nature()
            {
                Name = "Serious",
                _description = "Nature description"
            };
        }
        public static IEnumerable<Mint> GetAllMints()
        {
            Console.WriteLine("Getting all Mints");
            foreach (Nature nature in GetAllNatures())
            {
                Mint newMint = new Mint()
                {
                    Name = $"{nature.Name} Mint",
                    AssociatedNature = nature
                };
                yield return newMint;
            }
        }
    }
}
