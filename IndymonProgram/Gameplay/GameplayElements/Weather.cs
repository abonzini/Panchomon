namespace Gameplay.GameplayElements
{
    public class Weather : SimulationElement
    {
        // Massive list of all of them and their effects (less friendly than storing in Json but allow for complex behaviours)
        public static IEnumerable<Weather> GetAllWeathers()
        {
            yield return new Weather()
            {
                Name = "No Weather",
                _description = "No particular weather in this dungeon."
            };
            yield return new Weather()
            {
                Name = "Flood",
                _description = "The dungeon is partly flooded this week. Expect water all over the floor."
            };
            yield return new Weather()
            {
                Name = "High Voltage Arcs",
                _description = "There's a constant discharge of electricty around the Plant that may hit Pokemon."
            };
            yield return new Weather()
            {
                Name = "Ion Deluge",
                _description = "The air is Ionized and smells funny. All Normal-type attacks become Electric-type."
            };
        }
    }
}
