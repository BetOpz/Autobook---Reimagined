using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BetfairNgClient.Json.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RaceStatusEnum
    {
        /// <summary>
        /// There is no data available for this race.
        /// </summary>
        DORMANT,
        /// <summary>
        /// The start of the race has been delayed.
        /// </summary>
        DELAYED,
        /// <summary>
        /// The horses are in the parade ring.
        /// </summary>
        PARADING,
        /// <summary>
        /// The horses are going down to the starting post.
        /// </summary>
        GOINGDOWN,
        /// <summary>
        /// The horses are going behind the stalls.
        /// </summary>
        GOINGBEHIND,
        /// <summary>
        /// The horses are at the post.
        /// </summary>
        ATTHEPOST,
        /// <summary>
        /// The horses are loaded into the stalls/race is about to start.
        /// </summary>
        UNDERORDERS,
        /// <summary>
        /// The race has started.
        /// </summary>
        OFF,
        /// <summary>
        /// The race has finished.
        /// </summary>
        FINISHED,
        /// <summary>
        /// There has been a false start.
        /// </summary>
        FALSESTART,
        /// <summary>
        /// The result of the race is subject to a photo finish.
        /// </summary>
        PHOTOGRAPH,
        /// <summary>
        /// The result of the race has been announced.
        /// </summary>
        RESULT,
        /// <summary>
        /// The jockeys have weighed in.
        /// </summary>
        WEIGHEDIN,
        /// <summary>
        /// The race has been declared void.
        /// </summary>
        RACEVOID,
        /// <summary>
        /// The meeting has been cancelled.
        /// </summary>
        ABANDONED
    }
}
