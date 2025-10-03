using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetfairNgClient.Json.Enums
{
    public enum ResponseCodeEnum
    {
        /// <summary>
        /// Data returned successfully.
        /// </summary>
        OK,
        /// <summary>
        /// No updates since the passes UpdateSequence.
        /// </summary>
        NO_NEW_UPDATES,
        /// <summary>
        /// Event scores are no longer available or are not on the schedule.
        /// </summary>
        NO_LIVE_DATA_AVAILABLE,
        /// <summary>
        /// Data feed for the event type (tennis/football etc) is currently unavailable
        /// </summary>
        SERVICE_UNAVAILABLE,
        /// <summary>
        /// An unexpected error occurred retrieving score data.
        /// </summary>
        UNEXPECTED_ERROR,
        /// <summary>
        /// Live Data feed for this event/match is temporarily unavailable, data could potentially be stale.
        /// </summary>
        LIVE_DATA_TEMPORARY_UNAVAILABLE
    }
}
