namespace Gameplay.GameEngine
{
    public class Board
    {
        const int LAUNCHPAD_DEPTH = 3; // Add 3 extra for launchpad
        const int BOARD_DIM = 15; // Board is 15x15 (plus launchpads)
        public Tile[,] Grid = new Tile[LAUNCHPAD_DEPTH + BOARD_DIM, LAUNCHPAD_DEPTH + BOARD_DIM]; // All tiles are null but will be init
    }
}
