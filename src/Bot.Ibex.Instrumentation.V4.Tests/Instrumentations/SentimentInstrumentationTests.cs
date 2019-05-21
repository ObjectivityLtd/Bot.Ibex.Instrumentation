﻿namespace Bot.Ibex.Instrumentation.V4.Tests.Instrumentations
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using AutoFixture.Xunit2;
    using Bot.Ibex.Instrumentation.Common.Sentiments;
    using Bot.Ibex.Instrumentation.Common.Settings;
    using Bot.Ibex.Instrumentation.Common.Telemetry;
    using Bot.Ibex.Instrumentation.V4.Adapters;
    using Bot.Ibex.Instrumentation.V4.Instrumentations;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Bot.Schema;
    using Moq;
    using Objectivity.AutoFixture.XUnit2.AutoMoq.Attributes;
    using Xunit;

    [Collection("SentimentInstrumentation")]
    [Trait("Category", "Instrumentations")]
    public class SentimentInstrumentationTests
    {
        private const string FakeInstrumentationKey = "FAKE-INSTRUMENTATION-KEY";
        private readonly Mock<ITelemetryChannel> mockTelemetryChannel = new Mock<ITelemetryChannel>();
        private readonly TelemetryClient telemetryClient;

        public SentimentInstrumentationTests()
        {
            var telemetryConfiguration = new TelemetryConfiguration(FakeInstrumentationKey, this.mockTelemetryChannel.Object);
            this.telemetryClient = new TelemetryClient(telemetryConfiguration);
        }

        [Theory(DisplayName = "GIVEN any activity WHEN TrackMessageSentiment is invoked THEN event telemetry is being sent")]
        [AutoMockData]
        public async void GIVENAnyActivity_WHENTrackEventIsInvoked_THENEventTelemetryIsBeingSent(
            double sentimentScore,
            IMessageActivity activity,
            ISentimentClient sentimentClient,
            InstrumentationSettings settings)
        {
            // Arrange
            var instrumentation = new SentimentInstrumentation(sentimentClient, this.telemetryClient, settings);
            Mock.Get(sentimentClient)
                .Setup(s => s.GetSentiment(It.IsAny<ActivityAdapter>()))
                .Returns(Task.FromResult<double?>(sentimentScore));

            // Act
            await instrumentation.TrackMessageSentiment(activity)
                .ConfigureAwait(false);

            // Assert
            this.mockTelemetryChannel.Verify(
                tc => tc.Send(It.Is<EventTelemetry>(t =>
                    t.Name == EventTypes.MessageSentiment &&
                    t.Properties[SentimentConstants.Score] == sentimentScore.ToString(CultureInfo.InvariantCulture))),
                Times.Once);
        }

        [Theory(DisplayName = "GIVEN empty activity WHEN TrackMessageSentiment is invoked THEN exception is being thrown")]
        [AutoMockData]
        public async void GIVENEmptyActivity_WHENTrackMessageSentimentIsInvoked_THENExceptionIsBeingThrown(
            ISentimentClient sentimentClient,
            InstrumentationSettings settings)
        {
            // Arrange
            const IMessageActivity emptyActivity = null;
            var instrumentation = new SentimentInstrumentation(sentimentClient, this.telemetryClient, settings);

            // Act
            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => instrumentation.TrackMessageSentiment(emptyActivity))
                .ConfigureAwait(false);
        }

        [Theory(DisplayName = "GIVEN empty sentiment client WHEN SentimentInstrumentation is constructed THEN exception is being thrown")]
        [AutoData]
        public void GIVENEmptySentimentClient_WHENSentimentInstrumentationIsConstructed_THENExceptionIsBeingThrown(
            InstrumentationSettings settings)
        {
            // Arrange
            const ISentimentClient emptySentimentClient = null;

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => new SentimentInstrumentation(emptySentimentClient, this.telemetryClient, settings));
        }

        [Theory(DisplayName = "GIVEN empty telemetry client WHEN SentimentInstrumentation is constructed THEN exception is being thrown")]
        [AutoMockData]
        public void GIVENEmptyTelemetryClient_WHENSentimentInstrumentationIsConstructed_THENExceptionIsBeingThrown(
            ISentimentClient sentimentClient,
            InstrumentationSettings settings)
        {
            // Arrange
            const TelemetryClient emptyTelemetryClient = null;

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => new SentimentInstrumentation(sentimentClient, emptyTelemetryClient, settings));
        }

        [Theory(DisplayName = "GIVEN empty settings WHEN SentimentInstrumentation is constructed THEN exception is being thrown")]
        [AutoMockData]
        public void GIVENEmptySettings_WHENSentimentInstrumentationIsConstructed_THENExceptionIsBeingThrown(
            ISentimentClient sentimentClient)
        {
            // Arrange
            const InstrumentationSettings emptySettings = null;

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => new SentimentInstrumentation(sentimentClient, this.telemetryClient, emptySettings));
        }
    }
}