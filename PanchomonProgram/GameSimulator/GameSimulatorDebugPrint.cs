using Gameplay.GameEngine;

namespace GameSimulator
{
    public partial class GameSimulator
    {
        /// <summary>
        /// Gets a font color given a background color
        /// </summary>
        /// <param name="bg">Background color</param>
        /// <returns>The Fg color that is most visible</returns>
        static ConsoleColor GetFontColor(ConsoleColor bg)
        {
            return bg switch
            {
                ConsoleColor.Black => ConsoleColor.White,
                ConsoleColor.DarkBlue => ConsoleColor.White,
                ConsoleColor.DarkGreen => ConsoleColor.White,
                ConsoleColor.DarkCyan => ConsoleColor.White,
                ConsoleColor.DarkRed => ConsoleColor.White,
                ConsoleColor.DarkMagenta => ConsoleColor.White,
                ConsoleColor.DarkYellow => ConsoleColor.Black,
                ConsoleColor.Gray => ConsoleColor.Black,
                ConsoleColor.DarkGray => ConsoleColor.White,
                ConsoleColor.Blue => ConsoleColor.White,
                ConsoleColor.Green => ConsoleColor.Black,
                ConsoleColor.Cyan => ConsoleColor.Black,
                ConsoleColor.Red => ConsoleColor.White,
                ConsoleColor.Magenta => ConsoleColor.White,
                ConsoleColor.Yellow => ConsoleColor.Black,
                ConsoleColor.White => ConsoleColor.Black,
                _ => throw new Exception("Invalid console color"),
            };
        }
        /// <summary>
        /// Draws the game grid as is currently visible, with its elements
        /// </summary>
        /// <param name="description">What to print in the end</param>
        void DrawGrid(string description = "")
        {
            Console.Clear();
            int xSize = _grid.GetLength(0);
            int ySize = _grid.GetLength(1);
            for (int i = 0; i < xSize; i++)
            {
                for (int j = 0; j < ySize; j++)
                {
                    Tile nextTile = _grid[i, j];
                    if (nextTile != null)
                    {
                        // First, the grid
                        Console.BackgroundColor = nextTile.Surface.DebugColor;
                        Console.ForegroundColor = GetFontColor(nextTile.Surface.DebugColor);
                        Console.SetCursorPosition(i, j);
                        Console.WriteLine("▢");
                    }
                }
            }
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(0, ySize + 1);
            Console.WriteLine(description);
        }
    }
}
