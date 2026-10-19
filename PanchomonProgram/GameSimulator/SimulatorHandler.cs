using Gameplay.GameEngine;
using Utilities;

namespace GameSimulator
{
    public static class SimulatorHandler
    {
        /// <summary>
        /// Simulates a full game, given a list of trainers. Edits the trainer and pokemon instances during the simulation to return current hp, etc
        /// </summary>
        /// <param name="trainers">Trainers who'll participate in the fight</param>
        /// <param name="parameters">Additional battle settings</param>
        /// <param name="rng">The rng for this game (determinism!)</param>
        /// TODO This should also receive a field setup instance to define how the field will look and work
        /// <returns>The outcome of this game + rendering queue</returns>
        public static GameOutcome SimulateGame(List<TrainerInstance> trainers, GameParameters parameters, Random rng)
        {
            // Random winner idk
            HashSet<Side> participatingTeams = [.. trainers.Select(t => t.Team)];
            Side winningTeam = GeneralUtilities.GetRandomPick([.. participatingTeams], rng);
            List<TrainerInstance> winningTrainers = [.. trainers.Where(p => p.Team == winningTeam)];
            return new GameOutcome()
            {
                WinningTeam = winningTeam,
            };
        }
    }
}
