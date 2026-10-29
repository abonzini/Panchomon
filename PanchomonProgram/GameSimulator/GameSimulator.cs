using Gameplay.GameEngine;
using Gameplay.GameplayElements;
using Utilities;

namespace GameSimulator
{
    public partial class GameSimulator
    {
        // Structural elements
        const int LAUNCHPAD_DEPTH = 3; // Add 3 extra for launchpad
        const int BOARD_DIM = 15; // Board is 15x15 (plus launchpads)
        readonly Tile[,] _grid = new Tile[2 * LAUNCHPAD_DEPTH + BOARD_DIM, 2 * LAUNCHPAD_DEPTH + BOARD_DIM]; // All tiles are null but will be init
        List<TrainerInstance> _trainers;
        GameParameters _parameters = new GameParameters();
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
            _trainers = trainers;
            _parameters = parameters;
            if (!_newSim) throw new Exception("Game was already simulated. Can't redo.");
            // First, assemble the battlefield
            AssembleBattlefield([.. _trainers.Select(t => t.Team).Distinct()], parameters.TerrainGenerator, parameters.FieldModifier);
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
        /// <summary>
        /// Assembles the battlefield for this game. Also can be used for experimenting
        /// </summary>
        /// <param name="sides">Which sides will be included (to create launchpad)</param>
        /// <param name="terrainGen">Which terrain generator to use</param>
        /// <param name="extraFieldMods">Additional field mods to add, which have extra params</param>
        /// <param name="rng">Rng to use</param>
        public void AssembleBattlefield(List<Side> sides, TerrainGenerator terrainGen, FieldModifier extraFieldMods, Random rng = null)
        {
            if (rng != null) _rng = rng; // Use default one if none yet
            // Override the ones in params (wouldn't matter if it's the whole flow, but does allow for some SICK DEBUG)
            _parameters.TerrainGenerator = terrainGen;
            _parameters.FieldModifier = extraFieldMods;
            _parameters.SidesPresent = sides;
            InitBattlefield();
        }
    }
}
