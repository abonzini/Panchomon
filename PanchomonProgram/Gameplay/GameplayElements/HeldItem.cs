namespace Gameplay.GameplayElements
{
    public class HeldItem : SimulationElement
    {
        // Properties
        public PokemonType CramType = PokemonType.NONE;
        // Held-item basic hooks
        public bool IsUseful(PokemonEntity mon)
        {
            return _isUsefulHook(this, mon);
        }
        Func<HeldItem, PokemonEntity, bool> _isUsefulHook { get; set; } = delegate (HeldItem item, PokemonEntity mon) { return true; }; // Implementation for a Held Item to see if it's worth equipping to a Pokemon
        static bool ResistBerryEvaluation(HeldItem item, PokemonEntity mon)
        {
            Dictionary<PokemonType, Dictionary<PokemonType, double>> typeChartLookup = GameplayElementsContainer.GameplayElementsContainer.GlobalData.DefensiveTypeChart;
            double mult = 1;
            foreach (PokemonType type in mon.Species.Types)
            {
                mult *= typeChartLookup[type][item.CramType]; // Since resist berry cram types are the same as the resist type, just use these as lookup
            }
            return mult >= 1; // Item is useful if mon doesn't resist a move
        }
        static bool DamageTypeBoostEvaluation(HeldItem item, PokemonEntity mon)
        {
            return mon.Moves.Any(m => m.Type == item.CramType); // TODO This needs to check if it's a damaging move
        }
        // Massive list of all of them and their effects (less friendly than storing in Json but allow for complex behaviours)
        public static string GetResistBerryOfType(PokemonType type)
        {
            string berryName = type switch
            {
                PokemonType.NORMAL => "Chilan",
                PokemonType.FIGHTING => "Chople",
                PokemonType.FLYING => "Coba",
                PokemonType.POISON => "Kebia",
                PokemonType.GROUND => "Shuca",
                PokemonType.ROCK => "Charti",
                PokemonType.BUG => "Tanga",
                PokemonType.GHOST => "Kasib",
                PokemonType.STEEL => "Babiri",
                PokemonType.FIRE => "Occa",
                PokemonType.WATER => "Passho",
                PokemonType.GRASS => "Rindo",
                PokemonType.ELECTRIC => "Wacan",
                PokemonType.PSYCHIC => "Payapa",
                PokemonType.ICE => "Yache",
                PokemonType.DRAGON => "Haban",
                PokemonType.DARK => "Colbur",
                PokemonType.FAIRY => "Roseli",
                _ => throw new Exception("Invalid pokemon type")
            };
            berryName += " Berry";
            return berryName;
        }
        public static string GetBoostingItemName(PokemonType type)
        {
            return type switch
            {
                PokemonType.NORMAL => "Silk Scarf",
                PokemonType.FIGHTING => "Black Belt",
                PokemonType.FLYING => "Sharp Beak",
                PokemonType.POISON => "Poison Barb",
                PokemonType.GROUND => "Soft Sand",
                PokemonType.ROCK => "Hard Stone",
                PokemonType.BUG => "Silver Powder",
                PokemonType.GHOST => "Spell Tag",
                PokemonType.STEEL => "Metal Coat",
                PokemonType.FIRE => "Charcoal",
                PokemonType.WATER => "Mystic Water",
                PokemonType.GRASS => "Miracle Seed",
                PokemonType.ELECTRIC => "Magnet",
                PokemonType.PSYCHIC => "Twisted Spoon",
                PokemonType.ICE => "Never-Melt Ice",
                PokemonType.DRAGON => "Dragon Fang",
                PokemonType.DARK => "Black Glasses",
                PokemonType.FAIRY => "Fairy Feather",
                _ => throw new Exception("Invalid pokemon type")
            };
        }
        public static IEnumerable<HeldItem> GetAllHeldItems()
        {
            Console.WriteLine("Getting all Held Items");
            // Type-repeated items
            foreach (PokemonType type in Enum.GetValues<PokemonType>())
            {
                if (type == PokemonType.NONE) continue;
                yield return new HeldItem()
                {
                    Name = GetResistBerryOfType(type),
                    CramType = type,
                    _description = $"Halves the damage of the first {type}-type move that strikes this Pokemon in a battle.",
                    _isUsefulHook = ResistBerryEvaluation
                };
            }
            foreach (PokemonType type in Enum.GetValues<PokemonType>())
            {
                if (type == PokemonType.NONE) continue;
                yield return new HeldItem()
                {
                    Name = GetName(type, "Gem"),
                    CramType = type,
                    _description = $"The first {type}-type move used by this Pokemon in battle will have its power boosted by 50%.",
                    _isUsefulHook = DamageTypeBoostEvaluation,
                };
            }
            foreach (PokemonType type in Enum.GetValues<PokemonType>())
            {
                if (type == PokemonType.NONE) continue;

                yield return new HeldItem()
                {
                    Name = GetBoostingItemName(type),
                    CramType = type,
                    _description = $"Boosts the power of {type}-type moves by 20%.",
                    _isUsefulHook = DamageTypeBoostEvaluation,
                };
            }
            // All other items
            yield return new HeldItem()
            {
                Name = "Cell Battery",
                CramType = PokemonType.ELECTRIC,
                _description = $"Decide effect later."
            };
            yield return new HeldItem()
            {
                Name = "Black Sludge",
                CramType = PokemonType.POISON,
                _description = $"Decide effect later.",
                _isUsefulHook = (_, mon) => { return mon.Species.Types.Contains(PokemonType.POISON); }
            };
            yield return new HeldItem()
            {
                Name = "Eject Button",
                CramType = PokemonType.STEEL,
                _description = $"Decide effect later."
            };
            yield return new HeldItem()
            {
                Name = "Eject Pack",
                CramType = PokemonType.STEEL,
                _description = $"Decide effect later."
            };
            yield return new HeldItem()
            {
                Name = "Light Ball",
                CramType = PokemonType.ELECTRIC,
                _description = $"Decide effect later.",
                _isUsefulHook = (_, mon) => { return mon.Species.Name == "Pikachu" || mon.Species.Name == "Pichu"; }
            };
        }
    }
}
