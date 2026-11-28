namespace Gameplay.GameplayElements
{
    public class StatusCondition : GameplayElement
    {
        // Massive list of all of them and their effects (less friendly than storing in Json but allow for complex behaviours)
        public bool Persists = false; /// Whether they persist post-battle
        public static IEnumerable<StatusCondition> GetAllNonVolatileStatus()
        {
            Console.WriteLine("Getting all Status");
            yield return new StatusCondition()
            {
                Name = "Paralysis",
                _description = "This Pokemon is Paralyzed, it's speed is 25% of its maximum.",
                Persists = true
            };
            yield return new StatusCondition()
            {
                Name = "Sleep",
                _description = "This Pokemon is asleep. It will remain inactive and sleeping until it wakes up.",
                Persists = true
            };
        }
    }
}