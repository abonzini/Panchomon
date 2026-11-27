namespace Gameplay.GameplayElements
{
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
    }
}
