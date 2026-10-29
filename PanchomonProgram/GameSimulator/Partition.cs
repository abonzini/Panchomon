using Gameplay.GameEngine;

namespace GameSimulator
{
    /// <summary>
    /// Describes rectangular areas and main tool to check adjacency, partitioning, etc. Always defined from tl->br. X,Y are inclusive
    /// </summary>
    internal class Partition
    {
        public int Id; // Id of partition
        public int BegX;
        public int BegY;
        public int EndX;
        public int EndY;
        public bool Alive = true; // A partition that is dead is not attempted to be connected by join ops
        public Partition(int begX, int begY, int endX, int endY)
        {
            BegX = begX;
            BegY = begY;
            EndX = endX;
            EndY = endY;
        }
        public int Width { get { return EndX - BegX + 1; } }
        public int Height { get { return EndY - BegY + 1; } }
        public int Surface { get { return Width * Height; } }
        /// <summary>
        /// Bit of a weird one to explain. Tells you if a rect can be connected to another via a straight bridge with a specific direction. Since squares come many sizes, system also returns which tile the overlap begins and which one it ends. Assumes rectangles are not engulfed
        /// </summary>
        /// <param name="other">Other rect to head to</param>
        /// <returns>A triple, Direciton plus the overlap begin/end (dpending if NS or EW direction, direction may be none</returns>
        public (Direction, int overlapBegin, int overlapEnd) GetRaycastableTowards(Partition other)
        {
            Direction resultingDirection = Direction.NONE;
            // Check X overlap
            int overlapStart = Math.Max(BegX, other.BegX);
            int overlapEnd = Math.Min(EndX, other.EndX);
            if (overlapStart <= overlapEnd) // One rectangle can then be projected via another in X
            {
                if (other.EndY < BegY) // other is at N!
                {
                    resultingDirection = Direction.N;
                }
                else // Assume there's no degenerate case where rectangles are overlapping
                {
                    resultingDirection = Direction.S;
                }
            }
            else // No X overlap
            {
                // Check EW for overlap
                overlapStart = Math.Max(BegY, other.BegY);
                overlapEnd = Math.Min(EndY, other.EndY);
                if (overlapStart <= overlapEnd) // Projectable via Y
                {
                    if (other.EndX < BegX)
                    {
                        resultingDirection = Direction.W; // Square is at west
                    }
                    else // Assume there's no degenerate case where rectangles are overlapping
                    {
                        resultingDirection = Direction.E;
                    }
                }
            }
            // Not connectable via NESW, connection is NONE, garbage out
            return (resultingDirection, overlapStart, overlapEnd);
        }
        /// <summary>
        /// Returns all coords included in this square
        /// </summary>
        /// <returns></returns>
        public IEnumerable<(int, int)> GetAllTileCoords()
        {
            for (int i = BegX; i <= EndX; i++)
            {
                for (int j = BegY; j <= EndY; j++)
                {
                    yield return (i, j);
                }
            }
        }
        /// <summary>
        /// Gets the absolute dimension in integer tiles for the desired orientation (horiz or vertical)
        /// </summary>
        /// <param name="dim">Dimension to check</param>
        /// <param name="orientation">Orientation to check</param>
        /// <returns>The dimension in tiles</returns>
        public int GetAbsoluteDimension(double dim, SplitOrientation orientation)
        {
            int ret;
            if (dim < 1)
            {
                int max = orientation switch
                {
                    SplitOrientation.HORIZONTAL => Width,
                    SplitOrientation.VERTICAL => Height,
                    _ => throw new Exception("Horizontal or Vertical are the only valid options here"),
                };
                ret = (int)Math.Round(dim * max);
            }
            else
            {
                ret = (int)Math.Round(dim);
            }
            return ret;
        }
        /// <summary>
        /// Gets the absolute index for x or y for this rectangle. Transforms <1 (proportionals) into the actual tile
        /// </summary>
        /// <param name="relOffset">Value to use</param>
        /// <param name="orientation">Which orientation (only supports horizontal, vertical)</param>
        /// <param name="brushSize">The brush size to adjust the offset amount (max/min) relative to this orientation</param>
        /// <returns></returns>
        public int GetAbsoluteOffset(double relOffset, SplitOrientation orientation, int brushSize)
        {
            int ret;
            (int dim, int offset) = orientation switch
            {
                SplitOrientation.HORIZONTAL => (Width, BegX),
                SplitOrientation.VERTICAL => (Height, BegY),
                _ => throw new Exception("Horizontal or Vertical are the only valid options here"),
            };
            if (relOffset < 1)
            {
                dim -= brushSize - 1; // If brush bigger, it's relative dimension shrinks (less of them fit)
                ret = (int)(dim * relOffset) + offset;
            }
            else
            {
                ret = (int)Math.Round(relOffset) + offset;
            }
            return ret;
        }
        public override string ToString()
        {
            return $"{Id} ({BegX}, {BegY})->({Width}x{Height})";
        }
    }
}
