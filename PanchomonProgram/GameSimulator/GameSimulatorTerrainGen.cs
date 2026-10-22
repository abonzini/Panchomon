using Gameplay.GameEngine;

namespace GameSimulator
{
    /// <summary>
    /// Describes rectangular areas and main tool to check adjacency, partitioning, etc. Always defined from tl->br. X,Y are inclusive
    /// </summary>
    internal class Partition
    {
        public int BegX;
        public int BegY;
        public int EndX;
        public int EndY;
        public Partition(int begX, int begY, int endX, int endY)
        {
            BegX = begX;
            BegY = begY;
            EndX = endX;
            EndY = endY;
        }
        /// <summary>
        /// To check if touching or adjacent (or degenerate cases like engulfing)
        /// </summary>
        /// <param name="other">Other rect to check</param>
        /// <returns>True if they are touching</returns>
        public bool IsTouching(Partition other)
        {
            return BegX <= other.EndX &&
                EndX >= other.BegX &&
                BegY <= other.EndY &&
                EndY >= other.BegY;
        }
        /// <summary>
        /// Returns all coords included in this square
        /// </summary>
        /// <returns></returns>
        public IEnumerable<(int, int)> GetAllTiles()
        {
            for (int i = BegX; i <= EndX; i++)
            {
                for (int j = BegY; j <= EndY; j++)
                {
                    yield return (i, j);
                }
            }
        }
    }
    internal class PartitionSpace
    {
        int _nextId = 0;
        readonly Dictionary<int, Partition> _partitions = [];
        readonly Dictionary<int, int> _setParenting = []; // Links any partition with the macro disjoint set linked to it (ideally they'd all have the same value as all linked)
        /// <summary>
        /// Adds a new partition to the scheme
        /// </summary>
        /// <param name="partition">Which partition to add</param>
        public void AddNewPartition(Partition partition)
        {
            int nextId = _nextId++;
            _partitions.Add(nextId, partition);
            _setParenting.Add(nextId, nextId); // Each party begins as it's own disjoint set to be bridged later if needed
        }
        /// <summary>
        /// Merges the partition space with the current one as the main one, shift the ids so they don't overlap
        /// </summary>
        /// <param name="other">New space to combine with this one</param>
        public void MergeSpace(PartitionSpace other)
        {
            int idBase = _nextId;
            int maxId = idBase;
            foreach (KeyValuePair<int, Partition> kvp in other._partitions) // Unify partitions
            {
                int newId = idBase + kvp.Key;
                if (newId > maxId) maxId = newId;
                _partitions.Add(newId, kvp.Value);
            }
            foreach (KeyValuePair<int, int> set in other._setParenting) // Unify disjoint sets
            {
                _setParenting.Add(set.Key + idBase, set.Value + idBase);
            }
            _nextId = maxId + 1; // Ok if need to add more (?!?!?) prepare the id
        }
    }
    public partial class GameSimulator
    {
        /// <summary>
        /// Initializes and draws battlefield, ready to start sim after
        /// </summary>
        void InitBattlefield()
        {
            PartitionSpace CurrentPartitionSpace = new PartitionSpace(); // Generate the macro level of child partitions
            List<Side> sidesToAdd = [Side.FIELD, Side.BOTTOM, Side.TOP, Side.LEFT, Side.RIGHT];
            foreach (Side side in sidesToAdd)
            {
                if (side == Side.FIELD || _trainers.Any(t => t.Team == side)) // Only initialize field and launchpads for valid players
                {
                    // Create the corresponding side partition and add
                    Partition nextParty = side switch
                    {
                        Side.FIELD => new Partition(LAUNCHPAD_DEPTH, LAUNCHPAD_DEPTH, BOARD_DIM + LAUNCHPAD_DEPTH, BOARD_DIM + LAUNCHPAD_DEPTH),
                        Side.BOTTOM => new Partition(LAUNCHPAD_DEPTH, BOARD_DIM + LAUNCHPAD_DEPTH, BOARD_DIM + LAUNCHPAD_DEPTH, BOARD_DIM + 2 * LAUNCHPAD_DEPTH),
                        Side.TOP => new Partition(LAUNCHPAD_DEPTH, 0, BOARD_DIM + LAUNCHPAD_DEPTH, LAUNCHPAD_DEPTH),
                        Side.LEFT => new Partition(0, LAUNCHPAD_DEPTH, LAUNCHPAD_DEPTH, BOARD_DIM + LAUNCHPAD_DEPTH),
                        Side.RIGHT => new Partition(BOARD_DIM + LAUNCHPAD_DEPTH, LAUNCHPAD_DEPTH, BOARD_DIM + 2 * LAUNCHPAD_DEPTH, BOARD_DIM + LAUNCHPAD_DEPTH),
                        _ => throw new Exception($"The side {side} is not a valid trainer side, something happened"),
                    };
                    CurrentPartitionSpace.AddNewPartition(nextParty);
                }
            }
        }
    }
}
