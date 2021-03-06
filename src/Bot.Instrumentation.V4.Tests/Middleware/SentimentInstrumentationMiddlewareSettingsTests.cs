﻿namespace Bot.Instrumentation.V4.Tests.Middleware
{
    using System;
    using AutoFixture.Xunit2;
    using Bot.Instrumentation.Common.Settings;
    using Bot.Instrumentation.V4.Middleware;
    using Xunit;

    [Collection("SentimentInstrumentationMiddlewareSettings")]
    [Trait("Category", "Middleware")]
    public class SentimentInstrumentationMiddlewareSettingsTests
    {
        [Theory(DisplayName = "GIVEN empty InstrumentationSettings and any SentimentInstrumentationMiddlewareSettings WHEN SentimentInstrumentationMiddleware is constructed THEN exception is being thrown")]
        [AutoData]
        public void GIVENEmptyInstrumentationSettingsAndAnySentimentClientSettings__WHENSentimentInstrumentationMiddlewareSettingsIsConstructed__THENExceptionIsBeingThrown(
            SentimentClientSettings sentimentClientSettings)
        {
            // Arrange
            const InstrumentationSettings emptyInstrumentationSettings = null;

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => new SentimentInstrumentationMiddlewareSettings(emptyInstrumentationSettings, sentimentClientSettings));
        }

        [Theory(DisplayName = "GIVEN empty InstrumentationSettings and any SentimentInstrumentationMiddlewareSettings WHEN SentimentInstrumentationMiddleware is constructed THEN exception is being thrown")]
        [AutoData]
        public void GIVENAnyInstrumentationSettingsAndEmptySentimentClientSettings__WHENSentimentInstrumentationMiddlewareSettingsIsConstructed__THENExceptionIsBeingThrown(
            InstrumentationSettings instrumentationSettings)
        {
            // Arrange
            const SentimentClientSettings emptySentimentClientSettings = null;

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => new SentimentInstrumentationMiddlewareSettings(instrumentationSettings, emptySentimentClientSettings));
        }
    }
}
