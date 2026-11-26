using Gameplay.GameEngine;
using Utilities;

namespace GameSimulator
{
    /// <summary>
    /// State machine that describes all an entity needs. All durations are in seconds
    /// </summary>
    public class InstanceStateMachine
    {
        // State machine
        public InstanceState State = InstanceState.START_PAHSE;
        public double StateDuration = 0;
        public InstanceState InterruptedState = InstanceState.NONE; // For states that are interrupted like flinch, etc
        public double InterruptedStateDuration = 0;
        // Location and movement related
        public Direction Facing = Direction.NONE; // Where is the entity facing (if any)
        public double StepWaitDuration = 0; // A timer that tells you when the next step's going to be
        // Move data
        public int Target = -1; // Keeps track of what mon is targeting, normally would be dynamic but some natures are fixated. Using entity Id
        // Others
        public Dictionary<string, int> AuxVariables = []; // Keeps a list of variables that may become useful for special effects and the sort, used by moves and stuff
    }
    public partial class GameSimulator
    {
        Dictionary<int, PokemonInstance> _ingameMonInstances = [];
        Dictionary<int, InstanceStateMachine> _ingameStateMachines = [];
        int _nextInstanceId = 0;
        /// <summary>
        /// Performs the battle simulation
        /// </summary>
        /// <returns>The winning team</returns>
        Side SimulateBattle()
        {
            // Init all mons in the field
            if (!RefreshAllTrainers()) throw new Exception("Game was over before it even began!");
            // TODO: Then, begin the loop of if battle still ongoing, do the refresh, etc, and keep the sync
            // Return the winner (TODO)
            return Side.FIELD;
        }
        /// <summary>
        /// Refreshes all trainer states, adds the mons to the field if needed as the trainer loses mons or whatever
        /// </summary>
        /// <returns>Whether the game can continue or is ended</returns>
        bool RefreshAllTrainers()
        {
            // Init each mon for a trainer until there's enough mons for them
            for (int i = 0; i < _activeTrainers.Count; i++)
            {
                TrainerInstance trainer = _activeTrainers[i];
                bool trainerStillAlive = UpdateTrainerMonsInField(trainer);
                if (!trainerStillAlive) // If trainer lost all their mons, then this trainer doesn't play anymore
                {
                    _activeTrainers.RemoveAt(i);
                    i--;
                }
            }
            return _activeTrainers.Select(t => t.Team).Distinct().Count() > 1; // Game still on as long as there's more than 1 team still in the fight
        }
        /// <summary>
        /// Checks a trainer and keeps adding mons in the launchpad until the desired number of mons in the field is fulfilled (also checks if a trainer lost)
        /// </summary>
        /// <param name="trainer">Which trainer to update</param>
        /// <returns>Whether the trainer can continue</returns>
        bool UpdateTrainerMonsInField(TrainerInstance trainer)
        {
            int nMons = _ingameMonInstances.Values.Where(p => p.AssociatedTrainer == trainer).Count(); // Check the already present mons in the field that are owned by this trainer
            List<(int, int)> thisTrainersLaunchpad = GetLaunchpadValidCoords(trainer.Team, trainer.AutoDistributeMons);
            while (nMons < trainer.MaxMonsInField) // If I can add more, I add
            {
                PokemonInstance nextMon = PrepareTrainersNextMon(trainer);
                if (nextMon != null) // If there's a valid mon to add, do so
                {
                    Tile validTile = thisTrainersLaunchpad.Select(l => GetTileIfValid(l.Item1, l.Item2)).Where(t => t != null && t.Occupier == null).FirstOrDefault(); // Gets the first unoccupied valid tile
                    if (validTile == null) throw new Exception("No more tile to place Pokemon, this should not happen!");
                    // Apply mon to tile and tile to mon
                    validTile.Occupier = nextMon;
                    nextMon.AssociatedTile = validTile;
                    nMons++;
                }
                else // No mon to add
                {
                    break; // ok we end here then
                }
            }
            return nMons > 0; // Trainer still in the race if they have any mons still in there
        }
        /// <summary>
        /// If trainer can add a mon, gets it and prepares it
        /// </summary>
        /// <param name="trainer">Which trainer to get mon from</param>
        /// <returns>The mon, or null if there's none</returns>
        PokemonInstance PrepareTrainersNextMon(TrainerInstance trainer)
        {
            PokemonInstance mon = trainer.PokemonInTeam.Where(p => p.HpPercent > 0 && !_ingameMonInstances.ContainsValue(p)).FirstOrDefault(); // Get all mons that can actually fight and not already init
            if (mon == null)
            {
                // If not null, can then initialize it
                mon.ShuffledMoves = GeneralUtilities.ShuffleList([.. mon.ActiveMoves], _rng); // Shuffle the moves and reset the picker index
                mon.MovePicker = 0;
                // Then create a state machine for the mon and add into db
                int id = _nextInstanceId++;
                _ingameMonInstances[id] = mon;
                _ingameStateMachines[id] = new InstanceStateMachine(); // Also create a brand new state machine for it
            }
            return mon;
        }
        /// <summary>
        /// Gets the tiles corresponding to a launchpad where to put mons
        /// </summary>
        /// <param name="side">Which side to try</param>
        /// <param name="autoArrange">If it's arranged evenly and in a pretty manner or just go in order</param>
        /// <returns>A list of tiles to try to place someone</returns>
        List<(int, int)> GetLaunchpadValidCoords(Side side, bool autoArrange)
        {
            Partition teamsLaunchpad = _namedPartitions[side];
            List<(int, int)> launchpadValidCoords = []; // Will check which are the valid tiles
            // Super todo
            if (side == Side.BOTTOM || side == Side.TOP)
            {
                int y = (teamsLaunchpad.EndY + teamsLaunchpad.BegY) / 2; // Get the y point (fixed)
                for (int x = teamsLaunchpad.BegX; x <= teamsLaunchpad.EndX; x++)
                {
                    launchpadValidCoords.Add((x, y));
                }
            }
            if (side == Side.LEFT || side == Side.RIGHT)
            {
                int x = (teamsLaunchpad.EndX + teamsLaunchpad.BegX) / 2; // Get the x point (fixed)
                for (int y = teamsLaunchpad.EndY; y >= teamsLaunchpad.BegY; y--) // Iterate but on the reverse (makes sense visually!)
                {
                    launchpadValidCoords.Add((x, y));
                }
            }
            else // Field is just the center partition
            {
                launchpadValidCoords = [.. teamsLaunchpad.GetAllTileCoords()];
                if (!autoArrange) GeneralUtilities.ShuffleList(launchpadValidCoords, _rng); // If not an even arrangement, at least randomize the coords
            }
            if (autoArrange) // Finally, if auto arranging, use my trademark (??) algorithm for evenly dividing and choosing the best next launchpad tile
            {
                // No idea if this works for any number though...
                List<(int, int)> arrangedCoords = [];
                for (int level = 0; arrangedCoords.Count < launchpadValidCoords.Count; level++) // Add each level in a bsp-ish way
                {
                    List<(int, int)> thisLevelList = [];
                    int n = 1 << level;
                    int offset = (launchpadValidCoords.Count - 1) / (n << 1); // Offset is half of the current chunk
                    int step = (launchpadValidCoords.Count - 1) / n;
                    for (int i = offset; i < launchpadValidCoords.Count; i += step) // Iterate while i still have element to check
                    {
                        thisLevelList.Add(launchpadValidCoords[i]); // Add next elem
                    }
                    // Finally, put in final list in an even symmetrical distro
                    n = thisLevelList.Count;
                    for (int i = 0; i < n / 2; i++)
                    {
                        arrangedCoords.Add(thisLevelList[i]);
                        int sym = n - i - 1;
                        if (i != sym) // If there's a mirror element, also add it first
                        {
                            arrangedCoords.Add(thisLevelList[sym]);
                        }
                    }
                }
                launchpadValidCoords = arrangedCoords; // ok now this is the result
            }
            return launchpadValidCoords;
        }
    }
}
