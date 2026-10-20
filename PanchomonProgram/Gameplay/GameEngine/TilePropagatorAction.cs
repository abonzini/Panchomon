namespace Gameplay.GameEngine
{
    /* TODO: Seems we need to redo: Partition results now in an "available square" where the players can do stuff, but not every tile belongs to
     => WallPartition adds wall and creates n squares, while spacepartitions don't necessarily?
     At the end, there can be a join option that finds all squares and joins them, allowing squares to be defined in spiral patterns and cool stuff
    Way of doing it: Some absolute SIDE operations, ALL operations (or field whatever) but then, Field -> party, its internal partitions can't know the macro, and so, this is always relative to own
    e.g. in WM: field splits in half (or like, 2.3) with pitfall. Adds monastery in the edge (square room part, square inside is isloated). Then, for the square inside (gen part), more party can be created and joined. Afterwards you NEED to join with yourself
    => Base actions will always refer to like each side (and there's unreachable squares anyway which don't need to be connected) but it'll be iterative within squares of influence

    Other actions could be to paint intersections or exception spaces

    And later then, some specific brush sizes and places for stuff

    TODO: Final logic to think about, how are you goign to draw midlines (or snap them but in a relative manner?)?
    */
    /// <summary>
    /// A machine that allows to override tiles with a pattern, to create procedural elements
    /// </summary>
    public class TilePropagatorAction
    {
    }
}
