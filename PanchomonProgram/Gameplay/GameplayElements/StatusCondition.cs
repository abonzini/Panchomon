namespace Gameplay.GameplayElements
{
    public class StatusCondition : GameplayElement
    {
        // Massive list of all of them and their effects (less friendly than storing in Json but allow for complex behaviours)
        public bool Permanent = false; /// Permanent ones remain forever and between battles unless specifically cured
        public static IEnumerable<StatusCondition> GetAllNonVolatileStatus()
        {
            Console.WriteLine("Getting all NV Status");
            yield return new StatusCondition()
            {
                Name = "Paralysis",
                _description = "Mon is Paralyzed, it's speed is 25% of its maximum",
                Permanent = true
            };
        }
    }
}