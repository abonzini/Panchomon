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
        FIELD = 0, // Field units are free for all
        BOTTOM = 1,
        TOP = 2,
        LEFT = 3,
        RIGHT = 4,
        ALL // Used as param only
    }
}
