using Gameplay.GameEngine;

namespace GameSimulator
{
    public partial class GameSimulator
    {
        GameParameters _parameters = new GameParameters();
        List<TrainerInstance> _activeTrainers;
        // State machine for simulation
        bool _newSim = true;
        Random _rng;
        /// <summary>
        /// Simulates game, progresses internal state machine until game ends
        /// </summary>
        /// <param name="rng">Rng to use</param>
        /// <returns></returns>
        public GameOutcome Simulate(List<TrainerInstance> trainers, GameParameters parameters, Random rng)
        {
            _rng = rng;
            _activeTrainers = [.. trainers];
            _parameters = parameters;
            if (!_newSim) throw new Exception("Game was already simulated. Can't redo.");
            // First, assemble the battlefield
            AssembleBattlefield([.. _activeTrainers.Select(t => t.Team).Distinct()], parameters.TerrainGenerator, parameters.BattleTags);
            // Everything ready to go so we sim the battle now
            Side winningTeam = SimulateBattle();
            // Finish sim
            _newSim = false;
            // Placeholder for outcome
            return new GameOutcome()
            {
                WinningTeam = winningTeam, // Whatever
            };
        }
        /// <summary>
        /// Assembles the battlefield for this game. Also can be used for experimenting
        /// </summary>
        /// <param name="sides">Which sides will be included (to create launchpad)</param>
        /// <param name="terrainGen">Which terrain generator to use</param>
        /// <param name="rng">Rng to use</param>
        public void AssembleBattlefield(HashSet<Side> sides, TerrainGenerator terrainGen, HashSet<string> tags, Random rng = null)
        {
            if (rng != null) _rng = rng; // Use default one if none yet
            // Override the ones in params (wouldn't matter if it's the whole flow, but does allow for some SICK DEBUG)
            _parameters.TerrainGenerator = terrainGen;
            _parameters.SidesPresent = sides;
            _parameters.BattleTags = tags;
            InitBattlefield();
        }
    }
}
