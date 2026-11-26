using Gameplay.GameEngine;
using Utilities;

namespace GameSimulator
{
    public partial class GameSimulator
    {
        Dictionary<int, PokemonInstance> _ingameMonInstances = [];
        int _nextInstanceId = 0;
        /// <summary>
        /// Performs the battle simulation
        /// </summary>
        /// <returns>The winning team</returns>
        Side SimulateBattle()
        {
            // Init all mons in the field
            if (!RefreshAllTrainers()) throw new Exception("Game was over before it even began!");
            bool battleFinished = false;
            double nextTimeDelta = 0;
            while (!battleFinished)
            {
                // Update all mons
                double thisDelta = nextTimeDelta;
                nextTimeDelta = float.MaxValue;
                foreach (PokemonInstance nextMon in _ingameMonInstances.Values)
                {
                    nextTimeDelta = Math.Min(nextTimeDelta, UpdateState(nextMon, thisDelta)); // Calculate which delta to do next
                }
                // Then update all everything else (projectiles, weather entities, etc)
                // TODO, no dict yet...
                // After all objects resolved, next pass involves checking if any mon still alive, check if game ends, etc
                foreach (int nextMonId in _ingameMonInstances.Keys)
                {
                    // Todo checks if mon alive, checks if trainer needs to refresh, checks if game is over due to highlighted mon dying
                }
            }
            // Return the winner (TODO)
            return Side.FIELD;
        }
        #region Init/Refresh
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
            bool gameContinues = _activeTrainers.Select(t => t.Team).Distinct().Count() > 1; // Game still on as long as there's more than 1 team still in the fight
            gameContinues &= (_parameters.FocusedSide != Side.ALL) && _activeTrainers.Any(t => t.Team == _parameters.FocusedSide); // Game will stop also if there's a focused side and no more trainer of this side
            return gameContinues;
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
                mon.ShuffledMoves = [.. mon.ActiveMoves];
                RefreshMonMoves(mon);
                // Then create a state machine for the mon and add into db
                int id = _nextInstanceId++;
                _ingameMonInstances[id] = mon;
                mon.Sm = new InstanceStateMachine(); // Also create a brand new state machine for it
            }
            return mon;
        }
        /// <summary>
        /// Refreshes a mon's moves, useful for when mon has used all it's options
        /// </summary>
        /// <param name="mon">Which mon to reset</param>
        void RefreshMonMoves(PokemonInstance mon)
        {
            GeneralUtilities.ShuffleList([.. mon.ShuffledMoves], _rng); // Shuffle the moves and reset the picker index
            mon.NextMovePicker = 0;
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
        #endregion
        #region State Machine Update
        /// <summary>
        /// Updates the state of a specific mon, including analyzing the state machine thoroughly
        /// </summary>
        /// <param name="instance">Instance to update</param>
        /// <param name="updateDelta">Update time for this next frame in seconds</param>
        /// <returns>This entity's next refresh time</returns>
        double UpdateState(PhysicalInstance instance, double updateDelta)
        {
            // First thing to do is check mobility of mon (see if need to enter new tile)
            if (instance.Sm.CanPlanMovement) // If instance is allowed to plan its own movement
            {
                instance.Sm.StepPercentage += updateDelta * instance.Sm.StepParam; // Speed times time is the distance it made
                if (instance.Sm.StepPercentage > 1)
                {
                    // TODO perform step (i.e. plan and queue the target step accordingly)
                    instance.Sm.StepPercentage -= 1; // Will do one step only. If state machine is working well, should do one step at a time and shouldn't backlog multiple steps
                }
            }
            // Execute whatever the current state needs to do
            InstanceState newState = ExecuteState(instance, updateDelta);
            // Finally, if there's a new state either caused by state transition, will execute its beginning (init)
            if (instance.Sm.State != newState)
            {
                // TODO... begin state
            }
            // Should be good? Return the needed update time for this sm
            return instance.Sm.GetNextUpdateTime();
        }
        /// <summary>
        /// Executes a state from a given state machine, i.e. engine does what's expected from this state, if anything
        /// </summary>
        /// <param name="instance">id of entity whose state updates</param>
        /// <param name="updateDelta">Update delta, to decide what to do and also update the timer if needed</param>
        /// <returns>The new state, if any</returns>
        InstanceState ExecuteState(PhysicalInstance instance, double updateDelta)
        {
            InstanceState newState = instance.Sm.State;
            switch (newState)
            {
                case InstanceState.START_PHASE:
                    instance.Sm.StateCounter -= updateDelta;
                    if (instance.Sm.StateCounter <= 0) // Time to go to prep stage!
                    {
                        // Time to switch into prep state, with the mon charging a move and all that
                        newState = InstanceState.PREPARATION;
                        PokemonInstance mon = (PokemonInstance)instance;
                        if (mon.NextMovePicker > mon.ShuffledMoves.Count) // If already cycled, then need to start all over
                        {
                            RefreshMonMoves(mon);
                        }
                        mon.Sm.StateCounter = mon.ShuffledMoves[mon.NextMovePicker].Bars; // Set bars
                        mon.Sm.StateParam = (GetMonStat(mon, MonStat.SPEED) / 500) + 0.1; // TODO Revise bar speed formula I think it was this
                    }
                    break;
                case InstanceState.END_PHASE:
                    instance.Sm.StateCounter -= updateDelta;
                    if (instance.Sm.StateCounter <= 0) // Time to go to start phase after applying end of turn effects and choosing next move
                    {
                        // TODO EOT effects
                        newState = InstanceState.START_PHASE;
                        PokemonInstance mon = (PokemonInstance)instance;
                        mon.NextMovePicker++;
                    }
                    break;
                case InstanceState.PREPARATION:
                    instance.Sm.StateCounter -= updateDelta * instance.Sm.StateParam; // Just load the move bar
                    if (instance.Sm.StateCounter <= 0) // Once the mon charges all bars, the mon switches automatically to targeting mode
                    {
                        newState = InstanceState.TARGETING;
                    }
                    break;
                case InstanceState.TARGETING:
                    break;
                case InstanceState.USING_MOVE:
                    break;
                case InstanceState.MOVE_FINISHED:
                    break;
                case InstanceState.FLINCH:
                    break;
                case InstanceState.SLEEP:
                    break;
                case InstanceState.AUTO_ADVANCE:
                    break;
                case InstanceState.JUMPING:
                    break;
                case InstanceState.BULLDOZING:
                    break;
                case InstanceState.NONE: // Unrecognised or unimplemented states go here and will throw until i do something about them
                default:
                    throw new Exception($"State {newState} not implemented");
            }
            return newState;
        }
        #endregion
    }
}
