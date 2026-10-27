using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Gameplay.GameEngine
{
    /// <summary>
    /// To define sizes and stuff. If Min/Max are >= 1, then we assume it's an absolute number, while if <1, then we assume it's all proportional to some dimension of current part
    /// </summary>
    public class PropagationDimension
    {
        public double Min { get; set; } = 0;
        public double Max { get; set; } = 0;
        public double Decay { get; set; } = 1; /// If min != max, then the selection is done on a decay, where 1 is a uniform distro and otherwise we control the bias
        public override string ToString()
        {
            return $"{Min} -> {Max} ({Decay})";
        }
    }
    /// <summary>
    /// Line orientation for walls, decorations
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SplitOrientation
    {
        ANY, /// Any, chosen at random
        HORIZONTAL, /// Always Horizontal
        VERTICAL, /// Always Vertical
        LARGEST, /// The largest of the two
        ANY_WEIGHTED, /// Like ANY but it weights w.r.t. the dimensions of the room, so that a long room will be partitioned properly but a square one is more random
    }
    /// <summary>
    /// The actions that can actually be performed
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PropagationAction
    {
        NONE, /// Unused ig
        SPLIT_LINE, /// Splits a parition in 2. LineOrientation defines which direction. Unique parameter specifies where the line will hit, either absolute offset (?) or percentage of part. Partitions 0,1 generated are left/right respectively.
        SPLIT_ROOM, /// Sub partitions as a rectangle in a X,Y area. LineOrientation defines which one will be the MainDimension and vice versa. The parameters will be Dim1 Dim2 Offset1 Offset2. Partition 0 is inside the square, while 0-8 are in order from up-left. Using empty brush allows for insetting while workign on partitions 1-9
        SPLIT_LINES, /// Like draw line, but is based on splitting the space in the desired direction into N pieces (firs dim). The second dim is an offset allows to move the walls a bit proportional to the original part's dimension but do be careful as they may flip.
        FILL, /// Fills a partition, marked as invalid too depending on fill
        JOIN_PARTITIONS, /// Joins all partitions with a hallway brush. If multiple partitions, there's a modified Kruskal algo what does it. First dim's DECAY controls the sorting power, with 0 making the sorting completely random, 1 being distance-weighted and N++ making the sorting absolute. Next param sets a min value of room connections to avoid many dead end rooms, 1 for dead ends but 2+ will attempt to force to connect to more; Values >=1 forces a min of those connections per node, as values <1 are probabilities
        PAINT_RANDOMLY, /// Paints randomly
        PAINT_AT_CENTER, /// Paints brush in center area
        PAINT_AT_CORNER, /// Paints brush in corner area
        PAINT_AT_EDGE /// Paints brush in edge area
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PropagationTarget
    {
        INSETS = -2, /// Operation applies to inset squares, 1-9
        ALL = -1, /// Operation will apply to all subdivisions
        CENTER = 0, /// Operation applies to center square OR the field OR first partition
        FIELD = CENTER,
        FIRST = CENTER,
        BOTTOM = 1, /// Operation applies to bottom launchpad OR second partition
        SECOND = BOTTOM,
        TOP = 2, /// Operation applies to top launchpad
        LEFT = 3, /// Op applies to left launchpad
        RIGHT = 4, /// Op applies to right launchpad
    }
    /// <summary>
    /// A machine that allows to override tiles with a pattern, to create procedural elements
    /// </summary>
    public class TilePropagatorAction
    {
        public PropagationTarget ChildTarget { get; set; } = PropagationTarget.ALL; // Children of partition are numbered, in base one they equal TOP/BOTTOM/ETC. -1 indicates the action to be done in all child partitions
        public double Chance { get; set; } = 1; // Chance for this step to be executed or skipped. If more than one, chance to do it again
        public TileBrush Brush { get; set; }
        public List<TilePropagatorAction> PartitionActions { get; set; } = null; // Further actions, e.g. if after a partition is made, what to do inside (can allow for other sub-partitions, etc)
        // What action to actually perform
        public PropagationAction Action { get; set; } = PropagationAction.NONE;
        public SplitOrientation SplitOrientation { get; set; } = SplitOrientation.ANY; // If the action requires an orientation
        // Controls all dimensional parameters
        public List<PropagationDimension> DimensionalParameters { get; set; } = new List<PropagationDimension>();
        public override string ToString()
        {
            return Action.ToString();
        }
    }
}
