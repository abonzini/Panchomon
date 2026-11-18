using Gameplay.GameplayElements;

namespace PanchomonBackend
{
    public static class ValidLineupGenerator
    {
        /// <summary>
        /// Gets a list of all the possible teams given a list of one or many team build constraints.
        /// </summary>
        /// <param name="trainer">Which trainer</param>
        /// <param name="nMons">Number of mons desired in the team</param>
        /// <param name="constraintSets">All the different valid constraints that apply separately, only one needs to succeed</param>
        /// <returns>List of possible team builds that satisfy constraints</returns>
        public static List<List<PokemonEntity>> GetTrainerUsableMons(TrainerEntity trainer, int nMons, List<Constraint> constraintSets)
        {
            List<List<PokemonEntity>> resultingBuilds = new List<List<PokemonEntity>>();
            foreach (Constraint constraint in constraintSets)
            {
                // Get the possible lineup for this constraint
                List<PokemonEntity> thisTeamBuild = GetPossibleBuild(trainer, constraint);
                if (thisTeamBuild.Count >= nMons) // Need to check now if I have enough options to build a team with these constraints
                {
                    resultingBuilds.Add(thisTeamBuild);
                }
            }
            return resultingBuilds;
        }
        /// <summary>
        /// For a given trainer, gives me all the possible teams they could build with the corresponding constraint sets
        /// </summary>
        /// <param name="trainer">Which trainer</param>
        /// <param name="constraint">The teambuild constraints</param>
        /// <returns>The teambuild that could satisfy all these</returns>
        static List<PokemonEntity> GetPossibleBuild(TrainerEntity trainer, Constraint constraint)
        {
            List<PokemonEntity> resultingBuild = new List<PokemonEntity>();
            foreach (PokemonEntity mon in trainer.Pokemon)
            {
                if (constraint.IsSatisfiedByPokemon(mon.Species)) // Check if mon would potentially satisfy constraint
                {
                    resultingBuild.Add(mon);
                }
            }
            return resultingBuild;
        }
        /// <summary>
        /// Gets a list of all the possible teams given a list of one or many team build constraints.
        /// </summary>
        /// <param name="npc">Which NPC</param>
        /// <param name="nMons">Number of mons desired in the team</param>
        /// <param name="constraintSets">All the different valid constraints that apply separately, only one needs to succeed</param>
        /// <returns>List of possible team builds that satisfy constraints</returns>
        public static List<List<PokemonSpecies>> GetNpcSpeciesSets(NpcTrainer npc, int nMons, List<Constraint> constraintSets)
        {
            List<List<PokemonSpecies>> resultingBuilds = new List<List<PokemonSpecies>>();
            foreach (Constraint constraint in constraintSets)
            {
                // Get the possible lineup for this constraint
                List<PokemonSpecies> thisTeamBuild = GetPossibleBuild(npc, constraint);
                if (thisTeamBuild.Count >= nMons) // Need to check now if I have enough options to build a team with these constraints
                {
                    resultingBuilds.Add(thisTeamBuild);
                }
            }
            return resultingBuilds;
        }
        /// <summary>
        /// For a given trainer, gives me all the possible teams they could build with the corresponding constraint sets
        /// </summary>
        /// <param name="npc">Which trainer</param>
        /// <param name="constraint">The teambuild constraints</param>
        /// <returns>The teambuild that could satisfy all these</returns>
        static List<PokemonSpecies> GetPossibleBuild(NpcTrainer npc, Constraint constraint)
        {
            List<PokemonSpecies> resultingBuild = new List<PokemonSpecies>();
            foreach (PokemonSpecies mon in npc.AvailablePokemon)
            {
                if (constraint.IsSatisfiedByPokemon(mon)) // Check if mon would potentially satisfy constraint
                {
                    resultingBuild.Add(mon);
                }
            }
            return resultingBuild;
        }
    }
}
