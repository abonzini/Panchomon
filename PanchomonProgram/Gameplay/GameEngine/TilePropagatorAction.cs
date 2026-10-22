namespace Gameplay.GameEngine
{
    /// <summary>
    /// To define sizes and stuff. If Min/Max are >= 1, then we assume it's a number of tiles, while if <1, then we assume it's all proportional
    /// </summary>
    public class PropagationDimension
    {
        public double Min { get; set; } = 1;
        public double Max { get; set; } = 1;
        public double Decay { get; set; } = 1; /// If min != max, then the selection is done on a decay, where 1 is a uniform distro and otherwise we control the bias
    }
    /// <summary>
    /// Line orientation for walls, decorations
    /// </summary>
    public enum LineOrientation
    {
        HORIZONTAL, /// Always Horizontal
        VERTICAL, /// Always Vertical
        ANY, /// Any, chosen at random
        SAME, /// Same as last last (parent's) one, to do multiple lines
        OPPOSITE, /// Opposite of the last (parent's) one, to do grids
        LARGEST /// The largest of the two
    }
    /// <summary>
    /// The actions that can actually be performed
    /// </summary>
    public enum PropagationAction
    {
        NONE, /// Unused ig
        DRAW_LINE, /// Paints a line in the desired area
        DRAW_RECTANGLE_WALL, /// Paints a rectangle in the desired X,Y area
        SUBDIVIDE_INSET, /// Subdivides the partition into squares and then insets the squares proportionally. The brush is the "new wall"
        KILL_PARTITION, /// Kills & Fills a partition, marked invalid to avoid connecting it
        JOIN_PARTITIONS, /// Joins all child partitions with a hallway brush. If multiple partitions, there's a modified Kruskal algo what does it, use strength to number of connections and stuff
        PAINT_AT_CENTER, /// Paints brush in center area
        PAINT_AT_CORNER, /// Paints brush in corner area
        PAINT_AT_EDGE /// Paints brush in edge area
    }
    /// <summary>
    /// A machine that allows to override tiles with a pattern, to create procedural elements
    /// </summary>
    public class TilePropagatorAction
    {
        public int ChildTarget { get; set; } = -1; // Children of partition are numbered, in base one they equal TOP/BOTTOM/ETC. -1 indicates the action to be done in all child partitions
        public double Chance { get; set; } = 1; // Chance for this step to be executed or skipped. If more than one, chance to do it again
        public List<TilePropagatorAction> PartitionActions { get; set; } = null; // Further actions, e.g. if after a partition is made, what to do inside (can allow for other sub-partitions, etc)
        // What action to actually perform
        public PropagationAction Action { get; set; } = PropagationAction.NONE;
        // Controls the location where this action is performed, may be an offset or whatever
        public PropagationDimension MainDimension { get; set; }
        public PropagationDimension SecondaryDimension { get; set; }
        // In some cases, for example to partition more than once, or to control brush size?
        public PropagationDimension MainStrength { get; set; }
        public PropagationDimension SecondaryStrength { get; set; }
        public bool SymmetricBrush { get; set; } = true; // Depends on what the action is, attempts to place symmetrically or do other aesthetic stuff
        public bool ForcePartition { get; set; } = false; // Normally a partition is generated if a brush with a solid ObjectSurface is used, but sometimes you want to terrain tile e.g. separate with water and want this to be also partitioned
    }
}
