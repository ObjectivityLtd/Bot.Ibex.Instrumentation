﻿namespace Bot.Instrumentation.V3.Instrumentations
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;

    public interface ISentimentInstrumentation
    {
        Task TrackMessageSentiment(IActivity activity);
    }
}
