namespace Gameplay.GameplayElements
{
    public class FieldModifier : GameplayElement
    {
        // Massive list of all of them and their effects (less friendly than storing in Json but allow for complex behaviours)
        public static IEnumerable<FieldModifier> GetAllFieldMods()
        {
            Console.WriteLine("Getting all Field Mods");
            yield return new FieldModifier()
            {
                Name = "No Weather",
                _description = "No particular weather in this dungeon."
            };
            yield return new FieldModifier()
            {
                Name = "Flood",
                _description = "The dungeon is partly flooded this week. Expect water all over the floor."
            };
            yield return new FieldModifier()
            {
                Name = "High Voltage Arcs",
                _description = "There's a constant discharge of electricty around the Plant that may hit Pokemon."
            };
            yield return new FieldModifier()
            {
                Name = "Ion Deluge",
                _description = "The air is Ionized and smells funny. All Normal-type attacks become Electric-type."
            };
        }
    }
}
