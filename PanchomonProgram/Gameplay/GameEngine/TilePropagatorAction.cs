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
    /// Direction for trajectories
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
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
        NW,
        END_CARDINAL,
        CENTER_HORIZONTAL,
        CENTER_VERTICAL,
        CENTER_LINEAR,
        CENTER_DIAGONAL,
        CENTER_HORIZONTAL_ABS,
        CENTER_VERTICAL_ABS,
        CENTER_LINEAR_ABS,
        CENTER_DIAGONAL_ABS
    }
    /// <summary>
    /// The actions that can actually be performed
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PropagationAction
    {
        NONE, /// Unused ig
        TEMPLATE, /// Instead of an action, invokes a template defined in Tag. All other fields are ignored except chance and the target
        SPLIT_LINE, /// Splits a parition in 2. LineOrientation defines which direction. Unique parameter specifies where the line will hit, either absolute offset (?) or percentage of part. Partitions 0,1 generated are left/right respectively.
        SPLIT_ROOM, /// Sub partitions as a rectangle in a X,Y area. LineOrientation defines which one will be the MainDimension and vice versa. The parameters will be Dim1 Dim2 Offset1 Offset2. Partition 0 is inside the square, while 0-8 are in order from up-left. Using empty brush allows for insetting while workign on partitions 1-9
        SPLIT_LINES, /// Like draw line, but is based on splitting the space in the desired direction into N pieces (firs dim). The second dim is an offset allows to move the walls a bit proportional to the original part's dimension but do be careful as they may flip.
        FILL, /// Fills a partition, marked as invalid too depending on fill
        JOIN_PARTITIONS, /// Joins all partitions with a hallway brush. If multiple partitions, there's a modified Kruskal algo what does it. First dim's DECAY controls the sorting power, with 0 making the sorting completely random, 1 being distance-weighted and N++ making the sorting absolute. Next param sets a min value of room connections to avoid many dead end rooms, 1 for dead ends but 2+ will attempt to force to connect to more; Values >=1 forces a min of those connections per node, as values <1 are probabilities
        PAINT_EVERYWHERE, /// Paints randomly. Has a param where >=1 means an absolute number of tiles painted. Values <1 means as a percentage of the tile surface of desired partition. There's also an optional offset param as n tiles offset
        PAINT_AT_CENTER, /// Same as above, but paints in the center of the room
        PAINT_AT_CORNER, /// Same as above, but paints in the corners. Uses directions to see which ones to add
        PAINT_AT_EDGES, /// Same as above, but paints in the edges of the room, uses directions to see which ones to add. Doesnt use corners
        PAINT_MID_EDGES, /// Same as above, but paints in the middle of the edges of the room only, uses directions to see which ones to add
        PATH_FROM_PARTITION, /// From a partition, extends a path within parent partition, for aesthetics that require a line somewhere. First param is the length, either % of max or absolute (capped). Then stride, as tile step or proportion of total steps. Finally the anchor stride, which is either an absolute count ot a relative count depending on l/stride. Extension stops if this breaks pathing
        FLOOD_OFFTSET, /// Expands the desired brush outwards. This one doesn't block existing paths. Param is the min outwards offset, then max, and then the decaying chance between these
        FLOOD_DRY, /// Flood Drting has a similar algo than flood filling but the other way around. Finds tiles of a surface and erases them, filling them with an adjacent surf instead
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PropagationTarget
    {
        ALL_LAUNCHPAD = -7, /// Operation applies to tiles that have the id of valid launchpads for this game
        ALL_BLANK_ROOMS = -6, /// Room applies to non-launchpad partitions of size > 1 (i.e. non hallway) that are tagged so an any/all nonmarked action won't apply to them again. Used for decorating unique rooms
        N_BLANK_ROOM = -5,
        ALL_SIMULTANEOUSLY = -4, /// Op applies to all targets at the same time so it only needs to be ran once with whatever target
        ALL_SHUFFLE = -3, /// All the partitions but a bit of a shufflerino beforehand
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
        public string Tag { get; set; } = ""; // Tag string, if nonempty, needs to be present in terrain gen params
        public double RoomDecorationPercent { get; set; } = 0; // For room decoration (room-based targets), which percent of them will have this one
        public TileBrush Brush { get; set; } = new TileBrush();
        public List<TilePropagatorAction> PartitionActions { get; set; } = null; // Further actions, e.g. if after a partition is made, what to do inside (can allow for other sub-partitions, etc)
        // What action to actually perform
        public PropagationAction Action { get; set; } = PropagationAction.NONE;
        public SplitOrientation SplitOrientation { get; set; } = SplitOrientation.ANY; // If the action requires an orientation
        // Controls all dimensional parameters
        public List<PropagationDimension> DimensionalParameters { get; set; } = new List<PropagationDimension>();
        public List<Direction> DirectionsToApply { get; set; } = new List<Direction>(); // For actions that propagate towards somewhere
        public bool Aggressive { get; set; } = false; // Defines whether a draw/fill ignores path blocking (careful as this may break the arena unless you're smart with the obstacle/paths used)
        public override string ToString()
        {
            return Action.ToString();
        }
    }
}
