using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Gameplay.GameEngine
{
    /// <summary>
    /// To define sizes and stuff. If Min/Max are >= 1, then we assume it's an absolute number, while if <1, then we assume it's all proportional to some dimension of current part
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
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LineOrientation
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
        DRAW_LINE, /// Paints a line in the desired area. LineOrientation defines which direction. MainDimension specifies where the line will hit, either absolute offset (?) or percentage of part. Brush defines wall brush and whether it partitions, and invarancy defines whether it can (random) roll to flip the partition
        DRAW_RECTANGLE, /// Paints a rectangle in the desired X,Y area
        DRAW_GRID, /// Like draw line, but in a patterned grid with 2 directions
        FILL, /// Fills a partition, marked as invalid too depending on fill
        INSET, /// Shrinks a partition to a new space. The brush is the "new wall"
        JOIN_PARTITIONS, /// Joins all partitions with a hallway brush. If multiple partitions, there's a modified Kruskal algo what does it, use strength to number of connections and stuff
        PAINT_RANDOMLY, /// Paints randomly
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
        public TileBrush Brush { get; set; }
        public List<TilePropagatorAction> PartitionActions { get; set; } = null; // Further actions, e.g. if after a partition is made, what to do inside (can allow for other sub-partitions, etc)
        // What action to actually perform
        public PropagationAction Action { get; set; } = PropagationAction.NONE;
        public LineOrientation LineOrientation { get; set; } = LineOrientation.ANY; // If the action requires an orientation
        // Controls the "size" of the action, sometimes doesn't apply
        public PropagationDimension MainDimension { get; set; }
        public PropagationDimension SecondaryDimension { get; set; }
        // Controls the "offset" of the action, sometimes doesn't apply
        public PropagationDimension MainOffset { get; set; }
        public PropagationDimension SecondaryOffset { get; set; }
        public override string ToString()
        {
            return Action.ToString();
        }
    }
}
