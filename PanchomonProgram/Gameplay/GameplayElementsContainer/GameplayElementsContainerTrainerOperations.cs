using Gameplay.GameplayElements;
using Utilities;

namespace Gameplay.GameplayElementsContainer
{
    public partial class GameplayElementsContainer
    {
        /// <summary>
        /// Gets the power level of a trainer given their status, useful for tournaments and other events where they participate
        /// </summary>
        /// <param name="rank">Rank of trainer</param>
        /// <returns>Power level for this rank</returns>
        static public double GetTrainerRankPowerLevel(TrainerRank rank)
        {
            return rank switch
            {
                TrainerRank.UNRANKED => 0.2,
                TrainerRank.GYM => 0.4,
                TrainerRank.ELITE4 => 0.6,
                TrainerRank.CHAMPION => 8,
                _ => 0, // Default no additional power
            };
        }
        /// <summary>
        /// Creates a brand new trainer entity based on the minimum neccesary things needed
        /// </summary>
        /// <param name="name">Name of trainer</param>
        /// <param name="pictureUrl">Picture URL (if any) for battle</param>
        /// <param name="pokemonToUse">Which Pokemon will be used</param>
        /// <param name="nMons">How many mons to use, this can be int.maxvalue to jsut use exactly all the pokemon</param>
        /// <param name="powerLevel">Power level of Pokemon to be applied after equipping items, this is a number between 0 and 1 equivalent to the chance of every improvement to be used</param>
        /// <param name="shinyChance">Shiny chance</param>
        /// <param name="itemsToUse">Which items to attempt to use. Only Move Disks and Held Items are considered, as all others are just subject to powerLevel</param>
        /// <param name="rng">Rng to use</param>
        /// <returns>A created npc entity</returns>
        public TrainerEntity CreateNpcEntity(string name, string pictureUrl, List<PokemonSpecies> pokemonToUse, int nMons, double powerLevel, int shinyChance, List<string> itemsToUse, Random rng)
        {
            // First, generate the trainer
            TrainerEntity newTrainer = new TrainerEntity()
            {
                Name = name,
                PictureUrl = pictureUrl,
                AutoTeam = true
            };
            // Shuffled list
            List<PokemonSpecies> allSpeciesShuffled = [.. pokemonToUse]; // Copy as it'll be shuffled
            GeneralUtilities.ShuffleList(allSpeciesShuffled, rng);
            // Iterator function that will re-generate allSpeciesShuffled
            IEnumerable<PokemonSpecies> RepeatPokemonGenerator()
            {
                int totalYielded = 0;
                int i = 0;
                while (totalYielded < nMons)
                {
                    if (i >= allSpeciesShuffled.Count) // If reached end, reset and reshuffle list
                    {
                        i = 0;
                        GeneralUtilities.ShuffleList(allSpeciesShuffled, rng);
                    }
                    totalYielded++;
                    yield return allSpeciesShuffled[i++];
                }
                yield break;
            }
            // Ok now I add mons one by one
            IEnumerable<PokemonSpecies> monIterator = (nMons == int.MaxValue) ? allSpeciesShuffled : RepeatPokemonGenerator();
            foreach (PokemonSpecies chosenSpecies in monIterator)
            {

                PokemonEntity newMon = GlobalData.GenerateBlankPokemon(chosenSpecies, rng.Next(0, shinyChance) == 0); // Instantiate blank Pokemon template with the corresponding shiny chance
                RandomizePokemon(newMon, rng); // Randomize the rest for this mon
                newTrainer.Pokemon.Add(newMon);
            }
            // Now that trainer has all mons, now comes both the setting of items AND applying remaining power level
            List<string> usableItems = [.. itemsToUse]; // Copy for shuffling
            GeneralUtilities.ShuffleList(usableItems, rng);
            foreach (string item in usableItems) // Will only equip but attempt only when item makes sense
            {
                ItemType itemType = GetItemType(item);
                List<PokemonEntity> validMons = itemType switch
                {
                    ItemType.MOVE_DISK => [.. newTrainer.Pokemon.Where(m => m.MoveDisks.Contains(null))], // Only equip to mons that have empty move slots
                    ItemType.HELD_ITEM => [.. newTrainer.Pokemon.Where(p => p.HeldItem == null && HeldItems[item].IsUseful(p))], // Only on mons who can actually use this shit and have no item already
                    _ => [] // No mons will attempt to equip this
                };
                PokemonEntity monToEquip = GeneralUtilities.GetRandomPick(validMons, rng); // Will apply to a random mon
                if (itemType == ItemType.MOVE_DISK) // Equip in first blank slot
                {
                    if (monToEquip.MoveDisks[0] == null)
                    {
                        monToEquip.MoveDisks[0] = GetMoveDisk(item);
                    }
                    else
                    {
                        monToEquip.MoveDisks[1] = GetMoveDisk(item); // Should never override as this would have not been a valid mon
                    }
                }
                if (itemType == ItemType.HELD_ITEM) // Equip in first blank slot
                {
                    monToEquip.HeldItem = HeldItems[item];
                }
            }
            // Finally, apply power level to remaining elements
            foreach (PokemonEntity mon in newTrainer.Pokemon)
            {
                // First, moves. If move disk slot is empty, add blank disks
                for (int i = 0; i < mon.MoveDisks.Length; i++)
                {
                    if (mon.MoveDisks[i] == null && GeneralUtilities.GetRandomBool(powerLevel, rng)) // If move disk slot empty, add a blank disk depending on power luck
                    {
                        mon.MoveDisks[i] = GetMoveDisk(MoveDisk.BLANK_DISK);
                    }
                }
                // Then, abilities. Try enabling them
                for (int i = 0; i < mon.Abilities.Length; i++)
                {
                    if (mon.Abilities[i] != null && !mon.AbilityActive[i]) // A valid disabled ability can flip to active
                    {
                        mon.AbilityActive[i] = GeneralUtilities.GetRandomBool(powerLevel, rng);
                    }
                }
                // Finally, held items. Add basic held items such as gem (80%) or boosting item (20%) of their specific types if they're usable
                if (mon.HeldItem == null && GeneralUtilities.GetRandomBool(powerLevel, rng)) // Do i equip random item?
                {
                    // Check all potential equips
                    List<string> potentialEquips = GeneralUtilities.GetRandomBool(0.8, rng) ? [.. mon.Species.Types.Select(t => GameplayElement.GetName(t, "Gem"))] : [.. mon.Species.Types.Select(HeldItem.GetBoostingItemName)];
                    mon.HeldItem = GeneralUtilities.GetRandomPick([.. potentialEquips.Select(e => HeldItems[e]).Where(i => i.IsUseful(mon))], rng); // Equip an useful one, if any
                }
            }
            return newTrainer;
        }
        /// <summary>
        /// Consumes all items that the battle team has used. Auto replaced if there's items in inventory
        /// </summary>
        /// <param name="trainer">Trainer to consume</param>
        /// <param name="nMons">Mons whose items will be consumed (as not all would've participated in battle</param>
        public static void ConsumeTrainersItems(TrainerEntity trainer, int nMons = int.MaxValue)
        {
            // Verify mon items
            for (int i = 0; i < trainer.Pokemon.Count && i < nMons; i++)
            {
                PokemonEntity mon = trainer.Pokemon[i];
                // Gummies
                if (mon.AbilityActive[1])
                {
                    PokemonType gummyType = mon.Species.Types[0];
                    Gummy replacementGummy = trainer.Gummies.Keys.Where(g => g.Type == gummyType).FirstOrDefault(); // Can apply extra logic filters
                    if (replacementGummy != null) // Can use a gummy to keep active
                    {
                        GeneralUtilities.AddtemToCountDictionary(trainer.Gummies, replacementGummy, -1);
                    }
                    else // Otherwise, the ability can't be maintained anymore
                    {
                        mon.AbilityActive[1] = false;
                    }
                }
                if (mon.AbilityActive[2])
                {
                    PokemonType gummyType = (mon.Species.Types.Count > 1) ? mon.Species.Types[1] : mon.Species.Types[0];
                    Gummy replacementGummy = trainer.Gummies.Keys.Where(g => g.Type == gummyType).FirstOrDefault(); // Can apply extra logic filters
                    if (replacementGummy != null) // Can use a gummy to keep active
                    {
                        GeneralUtilities.AddtemToCountDictionary(trainer.Gummies, replacementGummy, -1);
                    }
                    else // Otherwise, the ability can't be maintained anymore
                    {
                        mon.AbilityActive[2] = false;
                    }
                }
                // Held Item
                if (mon.HeldItem != null)
                {
                    if (trainer.HeldItems.ContainsKey(mon.HeldItem))
                    {
                        GeneralUtilities.AddtemToCountDictionary(trainer.HeldItems, mon.HeldItem, -1);
                    }
                    else
                    {
                        mon.HeldItem = null;
                    }
                }
                // Move disks
                for (int j = 0; j < mon.MoveDisks.Length; j++)
                {
                    if (mon.MoveDisks[j] != null)
                    {
                        // Prioritize consuming blank disk and otherwise try to see if there's copies of the same disk
                        MoveDisk replacementDisk = trainer.MoveDisks.Keys.Where(d => d.IsSacrificial).FirstOrDefault();
                        if (replacementDisk == null && trainer.MoveDisks.ContainsKey(mon.MoveDisks[j])) replacementDisk = mon.MoveDisks[0];
                        if (replacementDisk != null) // Can use a gummy to keep active
                        {
                            GeneralUtilities.AddtemToCountDictionary(trainer.MoveDisks, replacementDisk, -1);
                        }
                        else // Otherwise, the ability can't be maintained anymore
                        {
                            mon.MoveDisks[j] = null;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Consumes an use of trainer's jewelry
        /// </summary>
        /// <param name="trainer">Trainer to check jewelry</param>
        public static void ConsumeTrainersJewelry(TrainerEntity trainer)
        {
            if (trainer.EquippedJewelry != null)
            {
                // Verify if jewelry ran out
                trainer.EquippedJewelryUses--;
                if (trainer.EquippedJewelryUses <= 0)
                {
                    trainer.EquippedJewelry = null;
                }
            }
        }
        /// <summary>
        /// Warns trainer if the number of items is close to max
        /// </summary>
        /// <param name="trainer">Trainer to warn</param>
        /// <returns>string with the specific warning</returns>
        public static string GetTrainerInventoryWarning(TrainerEntity trainer)
        {
            string result = "";
            static string WarnBag(int count, int max, int threshold, string what)
            {
                if (count > threshold)
                {
                    return $"You currently have {count}/{max} {what}.";
                }
                return "";
            }
            int threshold = 8 * TrainerEntity.MAX_NUMBER_BAG / 10; // Warning at 80% bag full
            result += WarnBag(trainer.Gummies.Count, TrainerEntity.MAX_NUMBER_BAG, threshold, "Gummies");
            result += WarnBag(trainer.MoveDisks.Count, TrainerEntity.MAX_NUMBER_BAG, threshold, "Move Disks");
            result += WarnBag(trainer.HeldItems.Count, TrainerEntity.MAX_NUMBER_BAG, threshold, "Held Items");
            result += WarnBag(trainer.Mints.Count, TrainerEntity.MAX_NUMBER_BAG, threshold, "Mints");
            result += WarnBag(trainer.EvoPlates.Count, TrainerEntity.MAX_NUMBER_BAG, threshold, "Evolution Plates");
            result += WarnBag(trainer.Essences.Count, TrainerEntity.MAX_NUMBER_BAG, threshold, "Essences");
            result += WarnBag(trainer.KeyItems.Count, TrainerEntity.MAX_NUMBER_BAG, threshold, "Key/Crafting Items");
            result += WarnBag(trainer.PokeBalls.Count, TrainerEntity.MAX_NUMBER_BAG, threshold, "PokeBalls");
            result += WarnBag(trainer.Sandwiches.Count, TrainerEntity.MAX_NUMBER_BAG, threshold, "Sandwiches");
            result += WarnBag(trainer.Favours.Count, TrainerEntity.MAX_NUMBER_BAG, threshold, "Favours");
            result += WarnBag(trainer.BoxedMons.Count, TrainerEntity.MAX_NUMBER_BAG, threshold, "Boxed Pokemon");
            if (result.Length > 0)
            {
                result += " Keep in mind that your bags can only hold up to {max} different items, and any additional items wont be picked up. You'll need to use, equip, discard or donate items if you want to make more space in your bag.";
            }
            return result;
        }
    }
}

