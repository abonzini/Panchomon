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
            return $"{Id} ({BegX}, {BegY})->({Width}x{Height})";
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
            partition.Id = nextId;
            _partitions.Add(nextId, partition);
            _setParenting.Add(nextId, nextId); // Each party begins as it's own disjoint set to be bridged later if needed
        }
        /// <summary>
        /// Removes a partition from this, mostly for when it's broken further
        /// </summary>
        /// <param name="partition"></param>
        public void RemovePartition(Partition partition)
        {
            _partitions.Remove(partition.Id);
            _setParenting.Remove(partition.Id);
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
                kvp.Value.Id = newId;
                _partitions.Add(newId, kvp.Value);
            }
            foreach (KeyValuePair<int, int> set in other._setParenting) // Unify disjoint sets
            {
                _setParenting.Add(set.Key + idBase, set.Value + idBase);
            }
            _nextId = maxId + 1; // Ok if need to add more, prepare the id
        }
        /// <summary>
        /// Gets how many child partitions are here
        /// </summary>
        /// <returns>How many</returns>
        public int GetPartitionCount()
        {
            return _partitions.Count;
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
        /// <summary>
        /// Gets (and also updates) the parenting of a node. The disjoint set's parent will be a single partition
        /// </summary>
        /// <param name="id">Id of partition to check</param>
        /// <returns>The partition that "owns" this disjoint set</returns>
        public int GetSetParent(int id)
        {
            if (_setParenting[id] != id)
            {
                _setParenting[id] = GetSetParent(_setParenting[id]); // Go deep until we get the original node, parent of this whole disjoint set
            }
            return _setParenting[id];
        }
        /// <summary>
        /// Joins id1 and id2 partitions
        /// </summary>
        /// <param name="id1">Partition 1</param>
        /// <param name="id2">Parition 2</param>
        /// <returns>The id</returns>
        public int Join(int id1, int id2)
        {
            int firstId = _setParenting[id1];
            int secondId = _setParenting[id2];
            int minId = Math.Min(_setParenting[id1], _setParenting[id2]);
            foreach (KeyValuePair<int, int> kvp in _setParenting.Where(p => p.Value == firstId || p.Value == secondId || p.Value == minId)) // Collapse recursively, I dont care if it's not efficient im tired boss
            {
                _setParenting[kvp.Key] = minId;
            }
            return minId;
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
            if (brushSize == 0) return;
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
                    foreach ((int x, int y) in nextParty.GetAllTileCoords()) // Paint this square with the basic brush
                    {
                        _grid[x, y] = new Tile(x, y); // Instance tile
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
                    List<Partition> targets = action.ChildTarget switch// (action.ChildTarget == -1) ? [.. currentPartitions.GetPartitions()] : [currentPartitions.GetPartition(action.ChildTarget)]; // Get all the partitions I'm going to work with
                    {
                        PropagationTarget.INSETS => [.. currentPartitions.GetPartitions().Where(p => p.Id >= 1 && p.Id <= 9)], // The inset pair
                        PropagationTarget.ALL => [.. currentPartitions.GetPartitions()], // All
                        _ => [currentPartitions.GetPartition((int)action.ChildTarget)], // Otherwise try to cast and fuck it
                    };
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
                            PropagationAction.JOIN_PARTITIONS => JoinPartitionsInSpace(currentPartitions, action), // Works to merge all current partitions
                            _ => throw new Exception($"Unsupported partition action {action.Action}")
                        };
                        if (newSpace.GetPartitionCount() > 0) // If the op caused more sub-partitions (childs) need to apply axtra actions and decide who remains
                        {
                            PropagateActions(action.PartitionActions, newSpace); // Apply further actions to the new spaces that were created
                            if (!partitionStillExists) // If this action caused this partition to be split or removed
                            {
                                currentPartitions.RemovePartition(target); // Then, removes parent since it was subdivided
                                currentPartitions.MergeSpace(newSpace); // Apply the new space to the merge space
                            }
                        }
                        Debug.DebugAction(DebugLevel.BATTLE_STATE, () => DrawGrid($"After {action.Action} on partition {target.Id}"));
                        if (action.Action == PropagationAction.JOIN_PARTITIONS) // This is an action that occurs in all partitions at once and doesn't subdivide, so it can skip once it passes once
                        {
                            break;
                        }
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
            return (brush.Occupier != null && brush.Occupier != "") || (brush.TileSurface != null && GameplayElementsContainer.GlobalData.Surfaces[brush.TileSurface].HindersMovement);
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
            int brushSize = brush.Size;
            if (brushSize == 0) return;
            for (int i = minX; i <= maxX; i += brushSize)
            {
                for (int j = minY; j <= maxY; j += brushSize)
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
            int brushSize = brush.Size;
            if (brushSize == 0) return;
            for (int i = startX; i < startX + width; i += brushSize)
            {
                ApplyBrushToTile(brush, i, startY);
                ApplyBrushToTile(brush, i, startY + height - 1);
            }
            for (int i = startY + brushSize; i < startX + width - 1; i += brushSize) // Remaining ones are smaller
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
            int dimension = GeneralUtilities.GetRandomDecayingInt(partition.GetAbsoluteOffset(parameters.DimensionalParameters[0].Min, orientation, brushSize),
                partition.GetAbsoluteOffset(parameters.DimensionalParameters[0].Max, orientation, brushSize),
                parameters.DimensionalParameters[0].Decay, _rng);
            // Apply the draw
            (int minX, int maxX) = (orientation == SplitOrientation.VERTICAL) ? (partition.BegX, partition.EndX) : (dimension, dimension); // Determine how x and y move
            (int minY, int maxY) = (orientation == SplitOrientation.HORIZONTAL) ? (partition.BegY, partition.EndY) : (dimension, dimension);
            ApplyBrushToRectangularRegion(brush, minX, minY, maxX, maxY);
            // Partition 0, from square until line (left/up)
            (minX, maxX) = (orientation == SplitOrientation.VERTICAL) ? (partition.BegX, partition.EndX) : (partition.BegX, dimension - brushSize);
            (minY, maxY) = (orientation == SplitOrientation.HORIZONTAL) ? (partition.BegY, partition.EndY) : (partition.BegY, dimension - brushSize);
            AddPartitionIfValid(minX, maxX, minY, maxY, newPartitions, 0);
            // Partition 1, from line until end (right/down)
            (minX, maxX) = (orientation == SplitOrientation.VERTICAL) ? (partition.BegX, partition.EndX) : (dimension + brushSize, partition.EndX);
            (minY, maxY) = (orientation == SplitOrientation.HORIZONTAL) ? (partition.BegY, partition.EndY) : (dimension + brushSize, partition.EndY);
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
            PropagationDimension nextDim = (orientation == SplitOrientation.HORIZONTAL) ? parameters.DimensionalParameters[0] : parameters.DimensionalParameters[1];
            int xDim = GeneralUtilities.GetRandomDecayingInt(partition.GetAbsoluteDimension(nextDim.Min, SplitOrientation.HORIZONTAL),
                partition.GetAbsoluteDimension(nextDim.Max, SplitOrientation.HORIZONTAL),
                nextDim.Decay, _rng);
            nextDim = (orientation == SplitOrientation.VERTICAL) ? parameters.DimensionalParameters[0] : parameters.DimensionalParameters[1];
            int yDim = GeneralUtilities.GetRandomDecayingInt(partition.GetAbsoluteDimension(nextDim.Min, SplitOrientation.VERTICAL),
                partition.GetAbsoluteDimension(nextDim.Max, SplitOrientation.VERTICAL),
                nextDim.Decay, _rng);
            nextDim = (orientation == SplitOrientation.HORIZONTAL) ? parameters.DimensionalParameters[2] : parameters.DimensionalParameters[3];
            int xBase = GeneralUtilities.GetRandomDecayingInt(partition.GetAbsoluteOffset(nextDim.Min, SplitOrientation.HORIZONTAL, xDim),
                partition.GetAbsoluteOffset(nextDim.Max, SplitOrientation.HORIZONTAL, xDim),
                nextDim.Decay, _rng);
            nextDim = (orientation == SplitOrientation.VERTICAL) ? parameters.DimensionalParameters[2] : parameters.DimensionalParameters[3];
            int yBase = GeneralUtilities.GetRandomDecayingInt(partition.GetAbsoluteOffset(nextDim.Min, SplitOrientation.VERTICAL, yDim),
                partition.GetAbsoluteOffset(nextDim.Max, SplitOrientation.VERTICAL, yDim),
                nextDim.Decay, _rng);
            // Finally, crop the square dimension so it never ever exceeds parent partition
            xDim = Math.Min(xDim, partition.Width - (xBase - partition.BegX));
            yDim = Math.Min(yDim, partition.Height - (yBase - partition.BegY));
            // Apply the draw of the wall
            TileBrush brush = parameters.Brush;
            ApplyBrushToRectangularBorders(brush, xBase, yBase, xDim, yDim);
            // Then partition the 9 spaces with the center one being 0. Mind the brush size as this sets the wall size, there may not be a wall
            int brushSize = brush.Size;
            // Partition 0 is the inside of the box
            (int minX, int maxX) = (xBase + brushSize, xBase + xDim - brushSize - 1); // An inset of the walls
            (int minY, int maxY) = (yBase + brushSize, yBase + yDim - brushSize - 1);
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
        /// <returns>Whether the element operated on still exists</returns>
        bool MultiLineDivide(Partition partition, TilePropagatorAction parameters, PartitionSpace newPartitions)
        {
            // Begin with the line examination, which tile does it begin? Horizontal-Vertical is flipped since it refers to the "line" orientation not the length of the rectangle
            SplitOrientation orientation = GetAbsolutePartitionOrientation(partition, parameters.SplitOrientation);
            TileBrush brush = parameters.Brush;
            if ((parameters.DimensionalParameters[0].Min >= 1) ^ (parameters.DimensionalParameters[0].Max >= 1)) // XOR to verify that both elements are in the same side of 1
            {
                throw new Exception("For a split the param min/max need to both be an integer or both be a fraction!");
            }
            int n = (parameters.DimensionalParameters[0].Min >= 1) ?
                GeneralUtilities.GetRandomDecayingInt(
                    (int)Math.Round(parameters.DimensionalParameters[0].Min),
                    (int)Math.Round(parameters.DimensionalParameters[0].Max),
                    parameters.DimensionalParameters[0].Decay, _rng) :
                GeneralUtilities.GetRandomDecayingInt( // Would be kinda stupid to do it this way. This split is better with integers
                    (int)Math.Round(1 / parameters.DimensionalParameters[0].Max),
                    (int)Math.Round(1 / parameters.DimensionalParameters[0].Min),
                    parameters.DimensionalParameters[0].Decay, _rng);
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
                int offsetNoise = GeneralUtilities.GetRandomDecayingInt(partition.GetAbsoluteDimension(parameters.DimensionalParameters[1].Min, orientation),
                partition.GetAbsoluteDimension(parameters.DimensionalParameters[1].Max, orientation),
                parameters.DimensionalParameters[1].Decay, _rng);
                wallIndex += GeneralUtilities.GetRandomBool(0.5, _rng) ? offsetNoise : -offsetNoise; // Add or remove this offset. This can work terribly bad if user is not extremely careful with offsets
                // Apply the next draw
                (minX, maxX) = (orientation == SplitOrientation.VERTICAL) ? (partition.BegX, partition.EndX) : (wallIndex, wallIndex); // Determine how x and y move
                (minY, maxY) = (orientation == SplitOrientation.HORIZONTAL) ? (partition.BegY, partition.EndY) : (wallIndex, wallIndex);
                ApplyBrushToRectangularRegion(brush, minX, minY, maxX, maxY);
                // Draw the next partition
                (minX, maxX) = (orientation == SplitOrientation.VERTICAL) ? (partition.BegX, partition.EndX) : (remainingChunkIndex, wallIndex - 1);
                (minY, maxY) = (orientation == SplitOrientation.HORIZONTAL) ? (partition.BegY, partition.EndY) : (remainingChunkIndex, wallIndex - 1);
                AddPartitionIfValid(minX, maxX, minY, maxY, newPartitions);
                remainingChunkIndex = wallIndex + brushSize; // Continue after the wall with the next one
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
        /// <returns>Whether the element operated on still exists</returns>
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
        /// <summary>
        /// Joins all the partitions inside the desired space to join
        /// </summary>
        /// <param name="spaceToJoin">Which space to join</param>
        /// <param name="parameters">Parameters that define how the partitions are joined</param>
        /// <returns>Always true, this operation never removes partitions</returns>
        bool JoinPartitionsInSpace(PartitionSpace spaceToJoin, TilePropagatorAction parameters)
        {
            // Get partitions to join and a map of tiles and which partition it belongs to
            List<Partition> partitions = [.. spaceToJoin.GetPartitions()]; // Get all partitions, raw, need to check what joins to what
            Dictionary<(int, int), int> tilePartDict = new Dictionary<(int, int), int>();
            Dictionary<int, int> connectionsPerNode = new Dictionary<int, int>();
            foreach (Partition partition in partitions)
            {
                foreach ((int, int) coords in partition.GetAllTileCoords())
                {
                    tilePartDict.Add(coords, partition.Id);
                }
                connectionsPerNode[partition.Id] = 0; // All parts begin unjoined
            }
            // Now the data structs needed for the tree
            HashSet<(int, int, int, Direction)> potentialConnections = new HashSet<(int, int, int, Direction)>(); // Contains all the potential valid connections that could be attempted. Id1->Id2, Distance, which direction from Id1
            Dictionary<(int, int), List<(int, int)>> potentialConectionValidStarts = new Dictionary<(int, int), List<(int, int)>>(); // Valid starts for joining 2 rectangles
            HashSet<(int, int, int, Direction)> knownPartitionStarts = new HashSet<(int, int, int, Direction)>();
            double sortingExponent = parameters.DimensionalParameters[0].Decay;
            for (int i = 0; i < partitions.Count; i++) // Check each part with each part
            {
                Partition partSource = partitions[i];
                if (!partSource.Alive) continue; // No joining dead partitions
                for (int j = i + 1; j < partitions.Count; j++)
                {
                    Partition partDest = partitions[j];
                    if (!partDest.Alive) continue; // No joining dead partitions
                    // Check if there's chance of overlap, and which tiles will do that, cause a raycast in each
                    (Direction connectionDirection, int overlapMin, int overlapMax) = partSource.GetRaycastableTowards(partDest);
                    if (connectionDirection != Direction.NONE)
                    {
                        // Ok they may join
                        (int minX, int maxX) = connectionDirection switch
                        {
                            Direction.N or Direction.S => (overlapMin, overlapMax),
                            Direction.E => (partSource.EndX, partSource.EndX),
                            Direction.W => (partSource.BegX, partSource.BegX),
                            _ => throw new Exception("Invalid straight direction")
                        };
                        (int minY, int maxY) = connectionDirection switch
                        {
                            Direction.E or Direction.W => (overlapMin, overlapMax),
                            Direction.N => (partSource.BegY, partSource.BegY),
                            Direction.S => (partSource.EndY, partSource.EndY),
                            _ => throw new Exception("Invalid straight direction")
                        };
                        // Some extra stuff to make sure we don't do degenerate tunnelling
                        (int, int) latDelta = GetLatDelta(connectionDirection);
                        // Check all tiles until I find at least 1 that touches, get the distance
                        for (int x = minX; x <= maxX; x++)
                        {
                            for (int y = minY; y <= maxY; y++)
                            {
                                int distance = 0;
                                (int, int, int, Direction) pathToCheck = (partSource.Id, x, y, connectionDirection);
                                if (knownPartitionStarts.Contains(pathToCheck)) continue; // Skip if path outcome known
                                foreach (Tile nextTile in RaycastSimple(x, y, connectionDirection, 1))
                                {
                                    (int, int) coord = (nextTile.X, nextTile.Y);
                                    if (!tilePartDict.TryGetValue(coord, out int tileOwnerPart)) // Check if no valid partition reached yet...
                                    {
                                        // Small trick to avoid tunneling longitudinally through a wall, accidentally connecting up to 4 rooms. Ideally can only tunnel if tunnel ONLY connects two
                                        bool canTunnel = !tilePartDict.ContainsKey((coord.Item1 + latDelta.Item1, coord.Item2 + latDelta.Item2)); // Basically, if tile dict marks adj tiles as non-owned, then its safe to tunnel as we won't accidentally join rooms
                                        canTunnel &= !tilePartDict.ContainsKey((coord.Item1 - latDelta.Item1, coord.Item2 - latDelta.Item2));
                                        if (canTunnel)
                                        {
                                            distance++;
                                        }
                                        else
                                        {
                                            knownPartitionStarts.Add(pathToCheck);
                                            break; // Else, need to abort this tunneling proposal, check the next path, whatever. Mark this path as known too
                                        }
                                    }
                                    else // Check if tile arrived to destination. Even if not, it will have arrived somewhere so may aswell register this path to avoid overtesting
                                    {
                                        // Found a potential one! Add if they're not already joined ofc
                                        if (distance > 0) // If they "could" be joined at some distance by a brush tunnel
                                        {
                                            potentialConnections.Add((partSource.Id, tileOwnerPart, distance, connectionDirection));
                                            if (!potentialConectionValidStarts.TryGetValue((partSource.Id, tileOwnerPart), out List<(int, int)> sourcesList))
                                            {
                                                sourcesList = new List<(int, int)>();
                                                potentialConectionValidStarts.Add((partSource.Id, tileOwnerPart), sourcesList);
                                            }
                                            sourcesList.Add((x, y));
                                        }
                                        else if (spaceToJoin.GetSetParent(partSource.Id) != spaceToJoin.GetSetParent(tileOwnerPart)) // If they're already joined, just connect the disjoint sets (if not yet)
                                        {
                                            spaceToJoin.Join(partSource.Id, tileOwnerPart);
                                            connectionsPerNode[partSource.Id]++;
                                            connectionsPerNode[tileOwnerPart]++;
                                        }
                                        else
                                        {
                                            // Already known to be joined
                                        }
                                        knownPartitionStarts.Add(pathToCheck); // In any case we know where this leads now so we don't need to check this path again
                                        knownPartitionStarts.Add((tileOwnerPart, coord.Item1, coord.Item2, GetOppositeDirection(connectionDirection))); // We also know that this path on the way back will end the same way so one less thing to check
                                        break; // Done, next path
                                    }
                                }
                            }
                        }
                    }
                }
            }
            // At this point I have a list of all the potential connections from everywhere to everywhere, so now it's time to sort by better and start joining. Distance negative to sort small distances first
            List<(int, int, int, Direction)> bestConnections = [.. potentialConnections.OrderBy(c => GeneralUtilities.GetSortableRandomWeight(Math.Pow(-c.Item3, sortingExponent), _rng))];
            foreach ((int, int, int, Direction) potentialConnection in bestConnections) // Now connect, one by one
            {
                // The elements that would force a path to be drawn no matter what
                double redundancyChance = 0;
                int nextNodesMinConnection = 1;
                PropagationDimension dimensionalParam = parameters.DimensionalParameters[1];
                if (dimensionalParam.Min >= 1 && dimensionalParam.Max >= 1)
                {
                    nextNodesMinConnection = GeneralUtilities.GetRandomDecayingInt(dimensionalParam.Min, dimensionalParam.Max, dimensionalParam.Decay, _rng);
                }
                else if (dimensionalParam.Min < 1 && dimensionalParam.Max < 1)
                {
                    redundancyChance = GeneralUtilities.GetRandomDecayingFloat(dimensionalParam.Min, dimensionalParam.Max, dimensionalParam.Decay, _rng);
                }
                else
                {
                    throw new Exception("Redundancy or forced min number, can't have a mix!");
                }
                bool forceDraw = (spaceToJoin.GetSetParent(potentialConnection.Item1) != spaceToJoin.GetSetParent(potentialConnection.Item2)); // Unjoined sets are always forced to join
                if (!forceDraw) // If not needed, can still try the other params for luck
                {
                    if (connectionsPerNode[potentialConnection.Item1] < nextNodesMinConnection || connectionsPerNode[potentialConnection.Item2] < nextNodesMinConnection)
                    {
                        forceDraw = true; // Will need to force min for these nodes anyway so do it
                    }
                    else // Otherwise just try the luck with the redundancy chance
                    {
                        forceDraw = GeneralUtilities.GetRandomBool(redundancyChance, _rng);
                    }
                }
                // Will join these 2 spaces if they are from disjoint sets OR we allow redundances
                if (forceDraw)
                {
                    List<(int, int)> possibleStarts = potentialConectionValidStarts[(potentialConnection.Item1, potentialConnection.Item2)];
                    GeneralUtilities.ShuffleList(possibleStarts, _rng); // Shuffle it, so we get random option
                    // FirstEvaluate that path can be made, and if so, apply it
                    bool connectionMade = false;
                    for (int i = 0; !connectionMade && i < possibleStarts.Count; i++) // Keep going until run out of possible connections or satisfied quota
                    {
                        (int x, int y) = possibleStarts[i];
                        (int, int) latDelta = GetLatDelta(potentialConnection.Item4); // Will check tunnels as I draw them because new tunnels may disallow some paths
                        List<(int, int)> tunnelingCoords = []; // These are the tiles we need to apply brush to
                        foreach (Tile nextTile in RaycastSimple(x, y, potentialConnection.Item4, parameters.Brush.Size)) // Start the raycast
                        {
                            (int, int) coord = (nextTile.X, nextTile.Y);
                            if (!tilePartDict.TryGetValue(coord, out int tileOwnerPart))
                            {
                                bool canTunnel = !tilePartDict.ContainsKey((coord.Item1 + latDelta.Item1, coord.Item2 + latDelta.Item2)); // Avoid adjacent tunnels to the same place (for visuals)
                                canTunnel &= !tilePartDict.ContainsKey((coord.Item1 - latDelta.Item1, coord.Item2 - latDelta.Item2));
                                if (canTunnel)
                                {
                                    tunnelingCoords.Add(coord); // Can do this one
                                }
                                else // If path broke due to tunneling, then try the next...
                                {
                                    break;
                                }
                            }
                            else if (tileOwnerPart == potentialConnection.Item1 || tileOwnerPart == potentialConnection.Item2) // If tile arrived to whatever it needed to
                            {
                                // Then thats it, path is completed
                                connectionMade = true; // An extra connection made
                                int id = spaceToJoin.Join(potentialConnection.Item1, potentialConnection.Item2); // Join the partitions if they weren't yet
                                connectionsPerNode[potentialConnection.Item1]++;
                                connectionsPerNode[potentialConnection.Item2]++;
                                foreach ((int, int) coordToPaint in tunnelingCoords) // Paint all the coords that need to be painted
                                {
                                    ApplyBrushToTile(parameters.Brush, coordToPaint.Item1, coordToPaint.Item2);
                                    tilePartDict[coordToPaint] = id; // Technically breaks with a big brush but it should never ever happen anyway so
                                }
                                break;
                            }
                            else // Path is not able to continue, so will need to try something else ig
                            {
                                break;
                            }
                        }
                    }
                }
            }
            // And thats it? By the end here everything should be joined methink
            return true;
        }
    }
}
