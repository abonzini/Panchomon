namespace Gameplay.GameEngine
{
    /// <summary>
    /// State machine of a simulated instance. Some will be auto-dealt with simulator but these can be controlled by moves and other effects which is why it's public here
    /// </summary>
    public enum InstanceState
    {
        NONE, /// No state, normally to show no state is being interrupted
        START_PHASE, /// Start phase, to calculate the next move and insta transitions to next stage
        END_PHASE, /// End phase of a mon, implements "end of turn" like effects from the game like leftovers, poison, etc
        PREPARATION, /// Stage where the mon is charing and preparing a move, may be moving somewhere in an async manner meanwhile, will last until fully charged
        TARGETING, /// Move is ready and charged. Mon continues to move. This lasts until a move timeout, or the move has been succesfully targeted, in which case it switches immediately to move preparation
        USING_MOVE, /// The moment a move is ready to be actually used. Most instant moves use this after a succesful target but some other complex moves may delay this
        MOVE_FINISHED, /// Hook to insta switch to End Phase, so the state machine knows when to do stuff
        // Interrupt states
        FLINCH, /// Mon is flinched, will resume state afterwards. Also applies for knockback, with flinch being a special type of knockback of distance 0
        SLEEP, /// Mon is sleeping, will resume state afterwards although the counter may be reset
        // Special movement states
        AUTO_ADVANCE, /// Auto advance at a specific speed using its own properties, useful for charging like volt tackles, flame wheels etc
        JUMPING, /// Mon is jumping with a fixed heading and speed, makes them also ungrounded
        BULLDOZING, /// Mon is moving forward like auto advance but reverts crashing meanings. Goes through physical destructible objects but stops when reaching a non destructible object (or no object)
    }
    /// <summary>
    /// State machine that describes all an entity needs. All durations are in seconds
    /// </summary>
    public class InstanceStateMachine
    {
        // State machine
        public InstanceState State = InstanceState.START_PHASE;
        public double StateParam = 0; // Some states keep this as the start value (time) others have a set time value so they just use this as a param
        public double StateCounter = 0;
        public InstanceState InterruptedState = InstanceState.NONE; // For states that are interrupted like flinch, etc
        public double InterruptedStateCounter = 0;
        // Location and movement related
        public Direction Facing = Direction.NONE; // Where is the entity facing (if any)
        public bool CanPlanMovement = false;
        public double StepParam = 0; // In steps per second
        public double StepPercentage = 0; // When reaches 1, a step is performed
        // Move data
        public int Target = -1; // Keeps track of what mon is targeting, normally would be dynamic but some natures are fixated. Using entity Id
        // Others
        public Dictionary<string, int> AuxVariables = []; // Keeps a list of variables that may become useful for special effects and the sort, used by moves and stuff
        /// <summary>
        /// Gets the next update time of an instance, given by its state machine
        /// </summary>
        /// <returns>Time needed to call this SM in time</returns>
        public double GetNextUpdateTime()
        {
            double nextUpdate;
            if (State == InstanceState.PREPARATION) // Preparation is the prep of a move, and thus will begin at the move's bars and decrease by the load speed
            {
                nextUpdate = StateCounter / StateParam; // Bars left divided bars per sec (will return how many seconds it'll take)
            }
            else // Otherwise it's just a timer
            {
                nextUpdate = StateCounter; // Next update for unit
            }
            if (CanPlanMovement) // If this one can also do conscious movement, check if the step can happen now
            {
                double walkUpdate = 1 - StepPercentage; // How much "steps" i need to do before moving
                if (walkUpdate < 0) walkUpdate = 0; // Capped to 0 to make the next step happen next one
                walkUpdate /= StepParam; // This becomes the time needed for the next step
                nextUpdate = Math.Min(nextUpdate, walkUpdate); // The update is whichever's sooner
            }
            return nextUpdate;
        }
    }
}
