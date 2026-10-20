using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Gameplay.GameEngine
{
    /// <summary>
    /// Defines the side of a trainer, where their mons will spawn. Also used to describe regions of the field for procedural generation
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Side
    {
        ALL = 0, /// Any field, any side
        FIELD = 1, /// Field units are free for all
        BOTTOM = 2,
        TOP = 3,
        LEFT = 4,
        RIGHT = 5,
    }
}
