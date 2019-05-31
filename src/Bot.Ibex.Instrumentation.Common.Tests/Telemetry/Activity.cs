﻿namespace Bot.Ibex.Instrumentation.Common.Tests.Telemetry
{
    using Bot.Ibex.Instrumentation.Common.Adapters;
    using Bot.Ibex.Instrumentation.Common.Models;

    public class Activity : IActivityAdapter
    {
        public string TimeStampIso8601 { get; set; }

        public string Type { get; set; }

        public string ChannelId { get; set; }

        public string ReplyToId { get; set; }

        public MessageActivity MessageActivity { get; set; }

        public ChannelAccount ChannelAccount { get; set; }
    }
}
