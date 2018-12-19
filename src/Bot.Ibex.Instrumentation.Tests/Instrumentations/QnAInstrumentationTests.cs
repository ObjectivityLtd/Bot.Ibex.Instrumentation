﻿namespace Bot.Ibex.Instrumentation.Tests.Instrumentations
{
    using System;
    using System.Globalization;
    using AutoFixture.Xunit2;
    using Bot.Ibex.Instrumentation.Instrumentations;
    using Bot.Ibex.Instrumentation.Telemetry;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Bot.Builder.AI.QnA;
    using Microsoft.Bot.Schema;
    using Moq;
    using Objectivity.AutoFixture.XUnit2.AutoMoq.Attributes;
    using Xunit;

    public class QnAInstrumentationTests
    {
        private const string FakeInstrumentationKey = "FAKE-INSTRUMENTATION-KEY";
        private readonly Mock<ITelemetryChannel> mockTelemetryChannel = new Mock<ITelemetryChannel>();
        private readonly TelemetryClient telemetryClient;

        public QnAInstrumentationTests()
        {
            var telemetryConfiguration = new TelemetryConfiguration(FakeInstrumentationKey, this.mockTelemetryChannel.Object);
            this.telemetryClient = new TelemetryClient(telemetryConfiguration);
        }

        [Theory(DisplayName = "GIVEN any activity WHEN TrackEvent is invoked THEN even telemetry sent")]
        [AutoMockData]
        public void GivenAnyActivity_WhenTrackEventIsInvoked_ThenEvenTelemetrySent(
            IMessageActivity activity,
            QueryResult queryResult,
            Settings settings)
        {
            // Arrange
            var instrumentation = new QnAInstrumentation(this.telemetryClient, settings);

            // Act
            instrumentation.TrackEvent(activity, queryResult);

            // Assert
            this.mockTelemetryChannel.Verify(tc => tc.Send(It.Is<EventTelemetry>(t =>
                t.Name == EventTypes.QnaEvent &&
                t.Properties[QnAConstants.UserQuery] == activity.Text &&
                t.Properties[QnAConstants.KnowledgeBaseQuestion] == string.Join(QnAInstrumentation.QuestionsSeparator, queryResult.Questions) &&
                t.Properties[QnAConstants.KnowledgeBaseAnswer] == queryResult.Answer &&
                t.Properties[QnAConstants.Score] == queryResult.Score.ToString(CultureInfo.InvariantCulture))));
        }

        [Theory(DisplayName = "GIVEN empty activity result WHEN TrackEvent is invoked THEN exception is thrown")]
        [AutoData]
        public void GivenEmptyActivity_WhenTrackEventIsInvoked_ThenExceptionIsThrown(
            QueryResult queryResult,
            Settings settings)
        {
            // Arrange
            var instrumentation = new QnAInstrumentation(this.telemetryClient, settings);
            const IMessageActivity emptyActivity = null;

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => instrumentation.TrackEvent(emptyActivity, queryResult));
        }

        [Theory(DisplayName = "GIVEN empty query result WHEN TrackEvent is invoked THEN exception is thrown")]
        [AutoMockData]
        public void GivenEmptyQueryResult_WhenTrackEventIsInvoked_ThenExceptionIsThrown(
            IMessageActivity activity,
            Settings settings)
        {
            // Arrange
            var instrumentation = new QnAInstrumentation(this.telemetryClient, settings);
            const QueryResult emptyQueryResult = null;

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => instrumentation.TrackEvent(activity, emptyQueryResult));
        }

        [Theory(DisplayName = "GIVEN empty telemetry client WHEN constructor is invoked THEN exception is thrown")]
        [AutoData]
        public void GivenEmptyTelemetryClient_WhenConstructorIsInvoked_ThenExceptionIsThrown(Settings settings)
        {
            // Arrange
            const TelemetryClient emptyTelemetryClient = null;

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => new QnAInstrumentation(emptyTelemetryClient, settings));
        }

        [Fact(DisplayName = "GIVEN empty settings WHEN constructor is invoked THEN exception is thrown")]
        public void GivenEmptySettings_WhenConstructorIsInvoked_ThenExceptionIsThrown()
        {
            // Arrange
            const Settings emptySettings = null;

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => new QnAInstrumentation(this.telemetryClient, emptySettings));
        }
    }
}