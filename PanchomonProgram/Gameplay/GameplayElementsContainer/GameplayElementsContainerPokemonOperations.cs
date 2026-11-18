using Gameplay.GameplayElements;
using Utilities;

namespace Gameplay.GameplayElementsContainer
{
    public partial class GameplayElementsContainer
    {
        /// <summary>
        /// Generates a random blank pokemon that can be later used as a template
        /// </summary>
        /// <param name="Species"></param>
        /// <returns></returns>
        PokemonEntity GenerateBlankPokemon(string Species)
        {
            bool isShiny = false;
            PokemonSpecies species;
            if (Species.Contains('★'))
            {
                isShiny = true;
                Species = Species.Split('★')[0].Trim(); // Get the actual species then
            }
            species = Dex[Species];
            return GenerateBlankPokemon(species, isShiny);
        }
        /// <summary>
        /// Generates a blank Pokemon template given a species and if it's shiny
        /// </summary>
        /// <param name="species">Species</param>
        /// <param name="isShiny">If shiny</param>
        /// <returns>A generated Pokemon Entity</returns>
        PokemonEntity GenerateBlankPokemon(PokemonSpecies species, bool isShiny)
        {
            PokemonEntity newPokemon = new PokemonEntity()
            {
                IsShiny = isShiny,
                Species = species,
                PokeBall = PokeBalls["Poke Ball"] // Use a default ball to avoid trouble
            };
            return newPokemon;
        }
        /// <summary>
        /// Full randomization of a Pokemon's battle stats and the sort, for the beginning of new games or when catching/fighting npcs
        /// </summary>
        /// <param name="mon">Mon to fully randomize (only battle stuff!)</param>
        /// <param name="rng">Rng to use</param>
        public void RandomizePokemon(PokemonEntity mon, Random rng)
        {
            // Naturally, some aspects like speices, nickname, pokeball are not changed here
            mon.Nature = GeneralUtilities.GetRandomPick([.. Natures.Values], rng);
            List<Move> availableMoves = [.. mon.Species.Moveset];
            List<Move> availableStabs = [.. availableMoves]; // TODO Can only find stabs once moves have more info ofc
            // Move 1 is always a stab (if available)
            if (availableStabs.Count > 0) mon.Moves[0] = GeneralUtilities.GetRandomPick(availableStabs, rng);
            if (mon.Moves[0] != null) availableMoves.Remove(mon.Moves[0]); // Remove from list, to reroll move 2
            // Reroll move 2
            if (availableMoves.Count > 0) mon.Moves[1] = GeneralUtilities.GetRandomPick(availableMoves, rng);
            // Then abilities, simpler
            List<Ability> availableAbilities = [.. mon.Species.Abilities];
            GeneralUtilities.ShuffleList(availableAbilities, rng); // Shuffle it
            mon.Abilities = [null, null, null]; // Blank list
            for (int i = 0; i < mon.Abilities.Length && i < availableAbilities.Count; i++) // Add as many as possible
            {
                mon.Abilities[i] = availableAbilities[i];
            }
        }
        /// <summary>
        /// Updates a specific pokemon, to add adittional moves/abilities when possible if new ones added
        /// </summary>
        /// <param name="mon">Mon to update</param>
        /// <param name="rng">Rng to use when shuffling</param>
        public static void UpdatePokemon(PokemonEntity mon, Random rng)
        {
            // Similar but need to remove the ones mon already has
            List<Move> availableMoves = [.. mon.Species.Moveset.Except(mon.Moves)];
            List<Move> availableStabs = [.. availableMoves.Except(mon.Moves)]; // TODO Can only find stabs once moves have more info ofc
            // Move 1 is always a stab (if available)
            if (mon.Moves[0] == null) // If mon has no move 0 yet, add a stab if possible
            {
                if (availableStabs.Count > 0) mon.Moves[0] = GeneralUtilities.GetRandomPick(availableStabs, rng);
                if (mon.Moves[0] != null) availableMoves.Remove(mon.Moves[0]); // Remove from list, to reroll move 2
            }
            // Reroll move 2
            if (mon.Moves[1] == null)
            {
                if (availableMoves.Count > 0) mon.Moves[1] = GeneralUtilities.GetRandomPick(availableMoves, rng);
            }
            // Then abilities, simpler
            List<Ability> availableAbilities = [.. mon.Species.Abilities.Except(mon.Abilities)];
            GeneralUtilities.ShuffleList(availableAbilities, rng); // Shuffle it
            for (int i = 0; i < mon.Abilities.Length && i < availableAbilities.Count; i++) // Add as many as possible into null slots
            {
                if (mon.Abilities[i] == null)
                {
                    mon.Abilities[i] = availableAbilities[i];
                }
            }
        }
        /// <summary>
        /// Adds a pokemon entity to the box system for temporary or permanent storage
        /// </summary>
        /// <param name="mon">Which mon to add</param>
        /// <param name="id">Id of mon to add</param>
        public void AddMonToBox(PokemonEntity mon, string id)
        {
            string newId = $"{mon.Name}-{id}"; // Uses this mon ID
            int idOffset = 0;
            while (!BoxedMons.ContainsKey(newId))
            {
                newId = $"{mon.Name}-{id}{++idOffset}"; // Add more and more until there's an index I can use
            }
            BoxedMons.Add(newId, mon);
        }
    }
}

