﻿namespace Bot.Instrumentation.V4.Instrumentations
{
    using System.Collections.Generic;
    using Bot.Instrumentation.Common.Telemetry;
    using Microsoft.Bot.Schema;

    public interface ICustomInstrumentation
    {
        void TrackCustomEvent(
            IActivity activity,
            string eventName = EventTypes.CustomEvent,
            IDictionary<string, string> properties = null);
    }
}
