namespace Gameplay.GameEngine
{
    /// <summary>
    /// State machine of a simulated instance. Some will be auto-dealt with simulator but these can be controlled by moves and other effects which is why it's public here
    /// </summary>
    public enum InstanceState
    {
        NONE, /// No state, normally to show no state is being interrupted
        START_PAHSE, /// Start phase, to calculate the next move and insta transitions to next stage
        END_PHASE, /// End phase of a mon, implements "end of turn" like effects from the game like leftovers, poison, etc
        PREPARATION, /// Stage where the mon is charing and preparing a move, may be moving somewhere in an async manner meanwhile, will last until fully charged
        TARGETING, /// Move is ready and charged. Mon continues to move. This lasts until a move timeout, or the move has been succesfully targeted, in which case it switches immediately to move preparation
        USING_MOVE, /// The moment a move is ready to be actually used. Most instant moves use this after a succesful target but some other complex moves may delay this
        MOVE_FINISHED, /// Hook to insta switch to End Phase, so the state machine knows when to do stuff
        // Interrupt states
        FLINCH, /// Mon is flinched, will resume state afterwards
        SLEEP, /// Mon is sleeping, will resume state afterwards although the counter may be reset

    }
}
