using Gameplay.GameEngine;
using Gameplay.GameplayElementsContainer;
using Utilities;

namespace GameSimulator
{
    /// <summary>
    /// Describes rectangular areas and main tool to check adjacency, partitioning, etc. Always defined from tl->br. X,Y are inclusive
    /// </summary>
    internal class Partition
    {
        public int id; // Id of partition
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
                ret = (int)(dim * max);
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
            return $"{BegX},{BegY} ({Width}x{Height})";
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
        /// <param name="id">Manual ID if needs to be specific</param>
        public void AddNewPartition(Partition partition, int id = -1)
        {
            int nextId = id == -1 ? _nextId : id;
            if (nextId < _nextId)
            {
                throw new Exception("Partition ID is lower than the internal counter and may be already occupied");
            }
            _nextId = nextId + 1; // The next one becomes the min possible
            partition.id = nextId;
            _partitions.Add(nextId, partition);
            _setParenting.Add(nextId, nextId); // Each party begins as it's own disjoint set to be bridged later if needed
        }
        /// <summary>
        /// Removes a partition from this, mostly for when it's broken further
        /// </summary>
        /// <param name="partition"></param>
        public void RemovePartition(Partition partition)
        {
            _partitions.Remove(partition.id);
            _setParenting.Remove(partition.id);
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
                kvp.Value.id = newId;
                _partitions.Add(newId, kvp.Value);
            }
            foreach (KeyValuePair<int, int> set in other._setParenting) // Unify disjoint sets
            {
                _setParenting.Add(set.Key + idBase, set.Value + idBase);
            }
            _nextId = maxId + 1; // Ok if need to add more, prepare the id
        }
        /// <summary>
        /// Gets a specific partition given its id
        /// </summary>
        /// <param name="id">Id of part</param>
        /// <returns>The partition that we look for</returns>
        public Partition GetPartition(int id)
        {
            return _partitions[id];
        }
        /// <summary>
        /// Gets all the partitions
        /// </summary>
        /// <returns>All the partitions</returns>
        public IEnumerable<Partition> GetPartitions()
        {
            return _partitions.Values;
        }
    }
    public partial class GameSimulator
    {
        /// <summary>
        /// Applies a brush to tile, overrides if "" does nothing if null. Applies ocuppier size tiling
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x">coord</param>
        /// <param name="y">coord</param>
        void ApplyBrushToTile(TileBrush brush, int x, int y)
        {
            int brushSize = brush.Size;
            bool overrideOcupier = brush.Occupier != null;
            PhysicalInstance newOccupierInstance = null;
            if (!string.IsNullOrEmpty(brush.Occupier))
            {
                newOccupierInstance = GameplayElementsContainer.GlobalData.PhysicalObjects[brush.Occupier].Clone();
                newOccupierInstance.X = x; // Object is in the first element of the tile, useful for when the object needs to remove itself or something
                newOccupierInstance.Y = y;
            }
            bool overrideTileEffect = brush.TileEffect != null;
            TileEffect newTileEffectInstance = null;
            if (!string.IsNullOrEmpty(brush.TileEffect))
            {
                newTileEffectInstance = GameplayElementsContainer.GlobalData.TileEffects[brush.TileEffect].Clone();
            }
            for (int i = x; i < x + brushSize; i += brushSize)
            {
                for (int j = y; j < y + brushSize; j += brushSize)
                {
                    Tile tile = _grid[i, j];
                    if (overrideOcupier) tile.Occupier = newOccupierInstance;
                    if (overrideTileEffect)
                    {
                        tile.Effect = newTileEffectInstance;
                        newTileEffectInstance.AssociatedTile = tile;
                    }
                    if (brush.TileSurface != null) tile.Surface = GameplayElementsContainer.GlobalData.Surfaces[brush.TileSurface]; // This should never be "no surface""
                    if (brush.CliffFace != null) tile.CliffFace = brush.CliffFace == "" ? CliffFace.NONE : Enum.Parse<CliffFace>(brush.CliffFace);
                }
            }
        }
        /// <summary>
        /// Initializes and draws battlefield, ready to start sim after
        /// </summary>
        /// <param name="sides">Which side(s) will be added</param>
        void InitBattlefield(List<Side> sides)
        {
            PartitionSpace CurrentPartitionSpace = new PartitionSpace(); // Generate the macro level of child partitions
            List<Side> sidesToAdd = [Side.FIELD, Side.BOTTOM, Side.TOP, Side.LEFT, Side.RIGHT];
            foreach (Side side in sidesToAdd)
            {
                if (side == Side.FIELD || sides.Contains(side)) // Only initialize field and launchpads for valid players
                {
                    // Create the corresponding side partition and add. Has -1 because beg-end is inclusive
                    Partition nextParty = side switch
                    {
                        Side.FIELD => new Partition(LAUNCHPAD_DEPTH, LAUNCHPAD_DEPTH, BOARD_DIM + LAUNCHPAD_DEPTH - 1, BOARD_DIM + LAUNCHPAD_DEPTH - 1),
                        Side.BOTTOM => new Partition(LAUNCHPAD_DEPTH, BOARD_DIM + LAUNCHPAD_DEPTH, BOARD_DIM + LAUNCHPAD_DEPTH - 1, BOARD_DIM + 2 * LAUNCHPAD_DEPTH - 1),
                        Side.TOP => new Partition(LAUNCHPAD_DEPTH, 0, BOARD_DIM + LAUNCHPAD_DEPTH - 1, LAUNCHPAD_DEPTH - 1),
                        Side.LEFT => new Partition(0, LAUNCHPAD_DEPTH, LAUNCHPAD_DEPTH - 1, BOARD_DIM + LAUNCHPAD_DEPTH - 1),
                        Side.RIGHT => new Partition(BOARD_DIM + LAUNCHPAD_DEPTH, LAUNCHPAD_DEPTH, BOARD_DIM + 2 * LAUNCHPAD_DEPTH - 1, BOARD_DIM + LAUNCHPAD_DEPTH - 1),
                        _ => throw new Exception($"The side {side} is not a valid trainer side, something happened"),
                    };
                    CurrentPartitionSpace.AddNewPartition(nextParty);
                    foreach ((int x, int y) in nextParty.GetAllTiles()) // Paint this square with the basic brush
                    {
                        _grid[x, y] = new Tile(); // Instance tile
                        ApplyBrushToTile(_parameters.TerrainGenerator.BasicBrush, x, y); // Apply the basic brush to this tile with all the things
                    }
                    Debug.DebugAction(DebugLevel.STEP_BY_STEP_SIM, () => DrawGrid($"Generated Partition {side}"));
                }
            }
            PropagateActions(_parameters.TerrainGenerator.PropagatorActions, CurrentPartitionSpace); // Now begin the iterative process of drawing the map
        }
        /// <summary>
        /// Propagates a series of tile propagation actions on the current partition list. This is the big one.
        /// If more parties are created, this function nests recursively, with the references 1,2, etc.
        /// </summary>
        /// <param name="actions">Which actions to perform on the parties</param>
        /// <param name="currentPartitions">Which partitions to apply the actions to. This may be edited while actions execute</param>
        void PropagateActions(List<TilePropagatorAction> actions, PartitionSpace currentPartitions)
        {
            if (actions == null) return; // Nothing to do here...
            Debug.DebugAction(DebugLevel.BATTLE_STATE, () => DrawGrid($"About to start next level of propagation"));
            foreach (TilePropagatorAction action in actions)
            {
                double chance = action.Chance;
                while (GeneralUtilities.GetRandomBool(chance, _rng)) // Roll to check if action applied
                {
                    // Execute action
                    List<Partition> targets = (action.ChildTarget == -1) ? [.. currentPartitions.GetPartitions()] : [currentPartitions.GetPartition(action.ChildTarget)]; // Get all the partitions I'm going to work with
                    // Party space can now be modified freely from this point onwards since targets was isolated. However user will need to be careful with op order as further tasks will also operate on generated partitions
                    foreach (Partition target in targets) // About to perform the action on each part
                    {
                        PartitionSpace newSpace = new PartitionSpace();
                        bool partitionStillExists = action.Action switch
                        {
                            PropagationAction.SPLIT_LINE => LineDivide(target, action, newSpace),
                            PropagationAction.SPLIT_ROOM => RectangularDivide(target, action, newSpace),
                            PropagationAction.SPLIT_LINES => MultiLineDivide(target, action, newSpace),
                            PropagationAction.FILL => FillPartition(target, action), // A fill never generates more things so
                            _ => throw new Exception($"Unsupported partition action {action.Action}")
                        };
                        PropagateActions(action.PartitionActions, newSpace); // Apply further actions to the new spaces that were created
                        if (!partitionStillExists) // If this action caused this partition to be split or removed
                        {
                            currentPartitions.RemovePartition(target); // Then, removes parent since it was subdivided
                            currentPartitions.MergeSpace(newSpace); // Apply the new space to the merge space
                        }
                        Debug.DebugAction(DebugLevel.BATTLE_STATE, () => DrawGrid($"After {action.Action} on partition {target.id}"));
                    }
                    chance--; // If >1, has a chance of going again. This is risky if the action involves sub-partitioning as this will sub partition everything
                }
            }
        }
        /// <summary>
        /// Returns whether a brush will generate a partition or not
        /// </summary>
        /// <param name="brush">Brush to check</param>
        /// <returns>Whether it's a "solid" separator brush</returns>
        static bool GeneratesPartition(TileBrush brush)
        {
            return (brush.Occupier != null && brush.Occupier != "") || (brush.TileSurface != null && GameplayElementsContainer.GlobalData.Surfaces[brush.TileSurface].AltersMovement);
        }
        /// <summary>
        /// Applies a specific brush to a line in the map
        /// </summary>
        /// <param name="brush">Which brush to apply</param>
        /// <param name="minX">Min X</param>
        /// <param name="minY">Min Y</param>
        /// <param name="maxX">Max X</param>
        /// <param name="maxY">Max Y</param>
        void ApplyBrushToRectangularRegion(TileBrush brush, int minX, int minY, int maxX, int maxY)
        {
            for (int i = minX; i <= maxX; i++)
            {
                for (int j = minY; j <= maxY; j++)
                {
                    ApplyBrushToTile(brush, i, j);
                }
            }
        }
        /// <summary>
        /// Applies a brush only on rectangular borders, so that remaining drawing is a O
        /// </summary>
        /// <param name="brush">Brush to apply</param>
        /// <param name="startX">Offset to begin</param>
        /// <param name="startY">Offset to begin</param>
        /// <param name="width">Size inclusive</param>
        /// <param name="height">Size inclusive</param>
        void ApplyBrushToRectangularBorders(TileBrush brush, int startX, int startY, int width, int height)
        {
            // Do it in 2 parts, first = and then || on the rest lol
            for (int i = startX; i < startX + width; i++)
            {
                ApplyBrushToTile(brush, i, startY);
                ApplyBrushToTile(brush, i, startY + height - 1);
            }
            for (int i = startY + 1; i < startX + width - 1; i++) // Remaining ones are smaller
            {
                ApplyBrushToTile(brush, startX, i);
                ApplyBrushToTile(brush, startX + width - 1, i);
            }
        }
        SplitOrientation GetAbsolutePartitionOrientation(Partition partition, SplitOrientation orientation)
        {
            return orientation switch
            {
                SplitOrientation.ANY => GeneralUtilities.GetRandomPick([SplitOrientation.HORIZONTAL, SplitOrientation.VERTICAL], _rng), // Random 50-50 chance
                SplitOrientation.HORIZONTAL => SplitOrientation.HORIZONTAL,
                SplitOrientation.VERTICAL => SplitOrientation.VERTICAL,
                SplitOrientation.LARGEST => (partition.Width > partition.Height) ? SplitOrientation.HORIZONTAL : SplitOrientation.VERTICAL, // Partition the largest one
                SplitOrientation.ANY_WEIGHTED => (GeneralUtilities.GetRandomWeightedIndex([partition.Width, partition.Height], _rng) == 0) ? SplitOrientation.HORIZONTAL : SplitOrientation.VERTICAL, // If width has more weight ot vice versa
                _ => throw new Exception($"{orientation} is not a valid known orientation")
            }; // The orientation i'll use depends on the desired parameter
        }
        /// <summary>
        /// Adds a partition to a space if the dimensions make sense
        /// </summary>
        /// <param name="minX">Dimension</param>
        /// <param name="maxX">Dimension</param>
        /// <param name="minY">Dimension</param>
        /// <param name="maxY">Dimension</param>
        /// <param name="spaceToAddTo">Where to add</param>
        /// <param name="desiredIndex">Index to use (or default one)</param>
        void AddPartitionIfValid(int minX, int maxX, int minY, int maxY, PartitionSpace spaceToAddTo, int desiredIndex = -1)
        {
            if (minX <= maxX && minY <= maxY) // If square exists, i.e. the partition line is such that there's still a part square, then generate it
            {
                spaceToAddTo.AddNewPartition(new Partition(minX, minY, maxX, maxY), desiredIndex);
            }
        }
        /// <summary>
        /// Divides a partition over a line
        /// </summary>
        /// <param name="partition">Which partition to divide</param>
        /// <param name="parameters">The parameter for this split, measured in actions</param>
        /// <param name="newPartitions">The Place where all new sub partitions is added, for extra stuff</param>
        /// <returns>Whether the element operated on still exists</returns>
        bool LineDivide(Partition partition, TilePropagatorAction parameters, PartitionSpace newPartitions)
        {
            // Begin with the line examination, which tile does it begin? Horizontal-Vertical is flipped since it refers to the "line" orientation not the length of the rectangle
            SplitOrientation orientation = GetAbsolutePartitionOrientation(partition, parameters.SplitOrientation);
            TileBrush brush = parameters.Brush;
            int brushSize = brush.Size;
            int dimension = GeneralUtilities.GetRandomDecayingNumber(partition.GetAbsoluteOffset(parameters.MainOffset.Min, orientation, brushSize),
                partition.GetAbsoluteOffset(parameters.MainOffset.Max, orientation, brushSize),
                parameters.MainOffset.Decay, _rng);
            // Apply the draw
            (int minX, int maxX) = (orientation == SplitOrientation.VERTICAL) ? (partition.BegX, partition.EndX) : (dimension, dimension); // Determine how x and y move
            (int minY, int maxY) = (orientation == SplitOrientation.HORIZONTAL) ? (partition.BegY, partition.EndY) : (dimension, dimension);
            ApplyBrushToRectangularRegion(brush, minX, minY, maxX, maxY);
            // Partition 0, from square until line (left/up)
            (minX, maxX) = (orientation == SplitOrientation.VERTICAL) ? (partition.BegX, partition.EndX) : (partition.BegX, dimension - 1);
            (minY, maxY) = (orientation == SplitOrientation.HORIZONTAL) ? (partition.BegY, partition.EndY) : (partition.BegY, dimension - 1);
            AddPartitionIfValid(minX, maxX, minY, maxY, newPartitions, 0);
            // Partition 1, from line until end (right/down)
            (minX, maxX) = (orientation == SplitOrientation.VERTICAL) ? (partition.BegX, partition.EndX) : (dimension + 1, partition.EndX);
            (minY, maxY) = (orientation == SplitOrientation.HORIZONTAL) ? (partition.BegY, partition.EndY) : (dimension + 1, partition.EndY);
            AddPartitionIfValid(minX, maxX, minY, maxY, newPartitions, 1);
            // Finally, returns whether this part should be removed
            return !GeneratesPartition(brush);
        }
        /// <summary>
        /// Divides a partition by placing a square with a wall in the middle, results in 9 partitions somehow, since the division is the square itself plus its potential 8 neighbors.
        /// Partition order is 0 for the generated one, plus 1-8 from topleft
        /// </summary>
        /// <param name="partition">Which partition to divide</param>
        /// <param name="parameters">The parameter for this split, measured in actions</param>
        /// <param name="newPartitions">The Place where all new sub partitions is added, for extra stuff</param>
        /// <returns>Whether the element operated on still exists</returns>
        bool RectangularDivide(Partition partition, TilePropagatorAction parameters, PartitionSpace newPartitions)
        {
            // Begin with the square examination, whats the dimension, and the offset? Get all, for X, Y
            SplitOrientation orientation = GetAbsolutePartitionOrientation(partition, parameters.SplitOrientation);
            // Converting potential relative dims into absolute X,Y ones (retainign which one's the main)
            PropagationDimension nextDim = (orientation == SplitOrientation.HORIZONTAL) ? parameters.MainDimension : parameters.SecondaryDimension;
            int xDim = GeneralUtilities.GetRandomDecayingNumber(partition.GetAbsoluteDimension(nextDim.Min, SplitOrientation.HORIZONTAL),
                partition.GetAbsoluteDimension(nextDim.Max, SplitOrientation.HORIZONTAL),
                nextDim.Decay, _rng);
            nextDim = (orientation == SplitOrientation.VERTICAL) ? parameters.MainDimension : parameters.SecondaryDimension;
            int yDim = GeneralUtilities.GetRandomDecayingNumber(partition.GetAbsoluteDimension(nextDim.Min, SplitOrientation.VERTICAL),
                partition.GetAbsoluteDimension(nextDim.Max, SplitOrientation.VERTICAL),
                nextDim.Decay, _rng);
            nextDim = (orientation == SplitOrientation.HORIZONTAL) ? parameters.MainOffset : parameters.SecondaryOffset;
            int xBase = GeneralUtilities.GetRandomDecayingNumber(partition.GetAbsoluteOffset(nextDim.Min, SplitOrientation.HORIZONTAL, xDim),
                partition.GetAbsoluteOffset(nextDim.Max, SplitOrientation.HORIZONTAL, xDim),
                nextDim.Decay, _rng);
            nextDim = (orientation == SplitOrientation.VERTICAL) ? parameters.MainOffset : parameters.SecondaryOffset;
            int yBase = GeneralUtilities.GetRandomDecayingNumber(partition.GetAbsoluteOffset(nextDim.Min, SplitOrientation.VERTICAL, yDim),
                partition.GetAbsoluteOffset(nextDim.Max, SplitOrientation.VERTICAL, yDim),
                nextDim.Decay, _rng);
            // Finally, crop the square dimensionso it never ever exceeds parent partition
            xDim = Math.Min(xDim, partition.Width - (xBase - partition.BegX));
            yDim = Math.Min(yDim, partition.Height - (yBase - partition.BegY));
            // Apply the draw
            TileBrush brush = parameters.Brush;
            ApplyBrushToRectangularBorders(brush, xBase, yBase, xDim, yDim);
            // Partition 0 is the inside of the box
            (int minX, int maxX) = (xBase + 1, xBase + xDim - 2); // An inset of the walls
            (int minY, int maxY) = (yBase + 1, yBase + yDim - 2);
            AddPartitionIfValid(minX, maxX, minY, maxY, newPartitions, 0);
            // Then the funny 8 ones all around
            // 1 = tl
            (minX, maxX) = (partition.BegX, xBase - 1);
            (minY, maxY) = (partition.BegY, yBase - 1);
            AddPartitionIfValid(minX, maxX, minY, maxY, newPartitions, 1);
            // 2 = t
            (minX, maxX) = (xBase, xBase + xDim - 1);
            (minY, maxY) = (partition.BegY, yBase - 1);
            AddPartitionIfValid(minX, maxX, minY, maxY, newPartitions, 2);
            // 3 = tr
            (minX, maxX) = (xBase + xDim, partition.EndX);
            (minY, maxY) = (partition.BegY, yBase - 1);
            AddPartitionIfValid(minX, maxX, minY, maxY, newPartitions, 3);
            // 4 = l
            (minX, maxX) = (partition.BegX, xBase - 1);
            (minY, maxY) = (yBase, yBase + yDim - 1);
            AddPartitionIfValid(minX, maxX, minY, maxY, newPartitions, 4);
            // 5 = r
            (minX, maxX) = (xBase + xDim, partition.EndX);
            (minY, maxY) = (yBase, yBase + yDim - 1);
            AddPartitionIfValid(minX, maxX, minY, maxY, newPartitions, 5);
            // 6 = bl
            (minX, maxX) = (partition.BegX, xBase - 1);
            (minY, maxY) = (yBase + yDim, partition.EndY);
            AddPartitionIfValid(minX, maxX, minY, maxY, newPartitions, 6);
            // 7 = tl
            (minX, maxX) = (xBase, xBase + xDim - 1);
            (minY, maxY) = (yBase + yDim, partition.EndY);
            AddPartitionIfValid(minX, maxX, minY, maxY, newPartitions, 7);
            // 8 = tl
            (minX, maxX) = (xBase + xDim, partition.EndX);
            (minY, maxY) = (yBase + yDim, partition.EndY);
            AddPartitionIfValid(minX, maxX, minY, maxY, newPartitions, 8);
            // Finally, returns whether this part should be removed
            return !GeneratesPartition(brush);
        }
        /// <summary>
        /// Divides the current partition into multiple N divisions which may then be offset slightly
        /// </summary>
        /// <param name="partition">Partition to divide</param>
        /// <param name="parameters">Params</param>
        /// <param name="newPartitions">Place where all new paritions will be put</param>
        /// <returns></returns>
        bool MultiLineDivide(Partition partition, TilePropagatorAction parameters, PartitionSpace newPartitions)
        {
            // Begin with the line examination, which tile does it begin? Horizontal-Vertical is flipped since it refers to the "line" orientation not the length of the rectangle
            SplitOrientation orientation = GetAbsolutePartitionOrientation(partition, parameters.SplitOrientation);
            TileBrush brush = parameters.Brush;
            if ((parameters.MainDimension.Min >= 1) ^ (parameters.MainDimension.Max >= 1)) // XOR to verify that both elements are in the same side of 1
            {
                throw new Exception("For a split the param min/max need to both be an integer or both be a fraction!");
            }
            int n = (parameters.MainDimension.Min >= 1) ?
                GeneralUtilities.GetRandomDecayingNumber(
                    (int)Math.Round(parameters.MainDimension.Min),
                    (int)Math.Round(parameters.MainDimension.Max),
                    parameters.MainDimension.Decay, _rng) :
                GeneralUtilities.GetRandomDecayingNumber( // Would be kinda stupid to do it this way. This split is better with integers
                    (int)Math.Round(1 / parameters.MainDimension.Max),
                    (int)Math.Round(1 / parameters.MainDimension.Min),
                    parameters.MainDimension.Decay, _rng);
            // Begin split
            int minX;
            int maxX;
            int minY;
            int maxY;
            double step = 1.0 / n; // How much the step'll be
            int brushSize = brush.Size;
            int remainingChunkIndex = partition.GetAbsoluteOffset(0, orientation, brushSize); // Get the index to start parititoning
            for (int i = 0; i < n - 1; i++) // Need to partition using n-1 walls
            {
                // Where would the wall be located in absolute terms?
                double nextWallLocation = step * (i + 1);
                int wallIndex = partition.GetAbsoluteOffset(nextWallLocation, orientation, brushSize);
                // Add some noise offset
                int offsetNoise = GeneralUtilities.GetRandomDecayingNumber(partition.GetAbsoluteDimension(parameters.MainOffset.Min, orientation),
                partition.GetAbsoluteDimension(parameters.MainOffset.Max, orientation),
                parameters.MainOffset.Decay, _rng);
                wallIndex += GeneralUtilities.GetRandomBool(0.5, _rng) ? offsetNoise : -offsetNoise; // Add or remove this offset. This can work terribly bad if user is not extremely careful with offsets
                // Apply the next draw
                (minX, maxX) = (orientation == SplitOrientation.VERTICAL) ? (partition.BegX, partition.EndX) : (wallIndex, wallIndex); // Determine how x and y move
                (minY, maxY) = (orientation == SplitOrientation.HORIZONTAL) ? (partition.BegY, partition.EndY) : (wallIndex, wallIndex);
                ApplyBrushToRectangularRegion(brush, minX, minY, maxX, maxY);
                // Draw the next partition
                (minX, maxX) = (orientation == SplitOrientation.VERTICAL) ? (partition.BegX, partition.EndX) : (remainingChunkIndex, wallIndex - 1);
                (minY, maxY) = (orientation == SplitOrientation.HORIZONTAL) ? (partition.BegY, partition.EndY) : (remainingChunkIndex, wallIndex - 1);
                AddPartitionIfValid(minX, maxX, minY, maxY, newPartitions);
                remainingChunkIndex = wallIndex + 1; // Continue after the wall with the next one
            }
            // After last wall, a final partition may remain
            (minX, maxX) = (orientation == SplitOrientation.VERTICAL) ? (partition.BegX, partition.EndX) : (remainingChunkIndex, partition.EndX);
            (minY, maxY) = (orientation == SplitOrientation.HORIZONTAL) ? (partition.BegY, partition.EndY) : (remainingChunkIndex, partition.EndY);
            AddPartitionIfValid(minX, maxX, minY, maxY, newPartitions);
            // Finally, returns whether this part should be removed
            return !GeneratesPartition(brush);
        }
        /// <summary>
        /// Fills a partition with a brush, invalidates it if the brush is non-movement
        /// </summary>
        /// <param name="partition">Which partition to fill</param>
        /// <param name="parameters">Params</param>
        /// <returns></returns>
        bool FillPartition(Partition partition, TilePropagatorAction parameters)
        {
            bool partitionKill = GeneratesPartition(parameters.Brush); // A brush that serves as a wall would kill the partition if it fills it
            ApplyBrushToRectangularRegion(parameters.Brush, partition.BegX, partition.BegY, partition.EndX, partition.EndY);
            if (partitionKill)
            {
                partition.Alive = false; // Affects the path connections
            }
            return !partitionKill;
        }
    }
}
