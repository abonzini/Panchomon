using Gameplay.GameEngine;
using Utilities;

namespace GameSimulator
{
    public partial class GameSimulator
    {
        // Structural elements
        const int LAUNCHPAD_DEPTH = 3; // Add 3 extra for launchpad
        const int BOARD_DIM = 15; // Board is 15x15 (plus launchpads)
        readonly Tile[,] _grid = new Tile[LAUNCHPAD_DEPTH + BOARD_DIM, LAUNCHPAD_DEPTH + BOARD_DIM]; // All tiles are null but will be init
        readonly List<TrainerInstance> _trainers;
        readonly GameParameters _parameters;
        // State machine for simulation
        bool _newSim = true;
        Random _rng;
        /// <summary>
        /// Initializes state machine
        /// </summary>
        /// <param name="trainers">Trainers who'll participate in the fight</param>
        /// <param name="parameters">Additional battle settings</param>
        public GameSimulator(List<TrainerInstance> trainers, GameParameters parameters)
        {
            _trainers = trainers;
            _parameters = parameters;
        }
        /// <summary>
        /// Simulates game, progresses internal state machine until game ends
        /// </summary>
        /// <param name="rng">Rng to use</param>
        /// <returns></returns>
        public GameOutcome Simulate(Random rng)
        {
            _rng = rng;
            if (!_newSim) throw new Exception("Game was already simulated. Can't redo.");
            // Random winner for now idc
            HashSet<Side> participatingTeams = [.. _trainers.Select(t => t.Team)];
            Side winningTeam = GeneralUtilities.GetRandomPick([.. participatingTeams], _rng);
            List<TrainerInstance> winningTrainers = [.. _trainers.Where(p => p.Team == winningTeam)];
            // Finish sim
            _newSim = false;
            return new GameOutcome()
            {
                WinningTeam = winningTeam,
            };
        }
    }
}
