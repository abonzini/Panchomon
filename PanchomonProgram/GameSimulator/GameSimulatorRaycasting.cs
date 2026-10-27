using Gameplay.GameEngine;

namespace GameSimulator
{
    public enum Direction
    {
        NONE,
        N,
        NE,
        E,
        SE,
        S,
        SW,
        W,
        NW
    }
    // This part deals with raycasting and tilewalking for aux game functionalities
    public partial class GameSimulator
    {
        /// <summary>
        /// Returns how X would increment in a specific direction
        /// </summary>
        /// <param name="direction">Which direction</param>
        /// <returns>X increment</returns>
        int GetXIncrement(Direction direction)
        {
            return direction switch
            {
                Direction.NONE => throw new Exception("Invalid direction to check"),
                Direction.E or Direction.NE or Direction.SE => 1,
                Direction.W or Direction.SW or Direction.NW => -1,
                _ => 0,
            };
        }
        /// <summary>
        /// Returns how X would increment in a specific direction
        /// </summary>
        /// <param name="direction">Which direction</param>
        /// <returns>X increment</returns>
        int GetYIncrement(Direction direction)
        {
            return direction switch
            {
                Direction.NONE => throw new Exception("Invalid direction to check"),
                Direction.N or Direction.NW or Direction.NE => -1,
                Direction.S or Direction.SW or Direction.SE => 1,
                _ => 0,
            };
        }
        /// <summary>
        /// Returns all tiles walking from a coord (X,Y) in a specific direction, for targeting and stuff
        /// </summary>
        /// <param name="x">X coord, wont be evaluated</param>
        /// <param name="y">y coord, wont be evaluated</param>
        /// <param name="direction">Direction to follow</param>
        /// <param name="stride">Stride of the raycast to check, i.e. tile separation</param>
        /// <returns>Each tile in the path</returns>
        IEnumerable<Tile> RaycastSimple(int x, int y, Direction direction, int stride)
        {

            int incX = GetXIncrement(direction);
            int maxX = _grid.GetLength(0);
            int incY = GetYIncrement(direction);
            int maxY = _grid.GetLength(1);
            int i = x + incX;
            int j = y + incY;
            while (i < maxX && j < maxY) // Iterate all tiles in that direction
            {
                yield return _grid[i, j];
                i += (stride * incX);
                j += (stride * incY);
            }
        }
        /// <summary>
        /// Gets the lateral delta X, Y from a direction in order to check adjacency while raycasting
        /// </summary>
        /// <param name="direction">Direction to check</param>
        /// <returns>Lateral delta positive term (subtract for the other one)</returns>
        (int, int) GetLatDelta(Direction direction)
        {
            return direction switch
            {
                Direction.N => (1, 0),
                Direction.NE => (1, 1),
                Direction.E => (0, 1),
                Direction.SE => (1, -1),
                Direction.S => (1, 0),
                Direction.SW => (1, 1),
                Direction.W => (0, 1),
                Direction.NW => (1, -1),
                _ => throw new Exception("Invalid direction to check movement"),
            };
        }
        /// <summary>
        /// Returns a dir opposite to the desired onw
        /// </summary>
        /// <param name="direction">Dir to check</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="Exception"></exception>
        Direction GetOppositeDirection(Direction direction)
        {
            return direction switch
            {
                Direction.N => Direction.S,
                Direction.NE => Direction.SW,
                Direction.E => Direction.W,
                Direction.SE => Direction.NW,
                Direction.S => Direction.N,
                Direction.SW => Direction.NE,
                Direction.W => Direction.E,
                Direction.NW => Direction.SE,
                _ => throw new Exception("Invalid direction to check movement"),
            };
        }
    }
}
