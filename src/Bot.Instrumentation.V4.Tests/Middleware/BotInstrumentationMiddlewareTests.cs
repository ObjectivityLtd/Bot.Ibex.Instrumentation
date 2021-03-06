﻿namespace Bot.Instrumentation.V4.Tests.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using AutoFixture.Xunit2;
    using Bot.Instrumentation.Common.Settings;
    using Bot.Instrumentation.Common.Telemetry;
    using Bot.Instrumentation.V4.Middleware;
    using FluentAssertions;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Moq;
    using Objectivity.AutoFixture.XUnit2.AutoMoq.Attributes;
    using Xunit;
    using ActivityTypes = Microsoft.Bot.Schema.ActivityTypes;

    [Collection("BotInstrumentationMiddleware")]
    [Trait("Category", "Middleware")]
    public class BotInstrumentationMiddlewareTests
    {
        private const string FakeInstrumentationKey = "FAKE-INSTRUMENTATION-KEY";
        private readonly Mock<ITelemetryChannel> mockTelemetryChannel = new Mock<ITelemetryChannel>();
        private readonly TelemetryClient telemetryClient;

        public BotInstrumentationMiddlewareTests()
        {
            var telemetryConfiguration = new TelemetryConfiguration(FakeInstrumentationKey, this.mockTelemetryChannel.Object);
            this.telemetryClient = new TelemetryClient(telemetryConfiguration);
        }

        [Theory(DisplayName = "GIVEN turn context with any activity WHEN OnTurnAsync is invoked THEN event telemetry is being sent")]
        [AutoMockData]
        public async void GIVENTurnContextWithAnyActivity_WHENOnTurnAsyncIsInvoked_THENEventTelemetryIsBeingSent(
            Activity activity,
            ITurnContext turnContext,
            InstrumentationSettings settings)
        {
            // Arrange
            var instrumentation = new BotInstrumentationMiddleware(this.telemetryClient, settings);
            Mock.Get(turnContext)
                .SetupGet(c => c.Activity)
                .Returns(activity);

            // Act
            await instrumentation.OnTurnAsync(turnContext, null)
                .ConfigureAwait(false);

            // Assert
            this.mockTelemetryChannel.Verify(tc => tc.Send(It.IsAny<EventTelemetry>()), Times.Once);
        }

        [Theory(DisplayName = "GIVEN turn context WHEN SendActivities is invoked THEN event telemetry is being sent")]
        [AutoMockData]
        public async void GIVENTurnContext_WHENSendActivitiesInvoked_THENEventTelemetryIsBeingSent(
            InstrumentationSettings settings,
            ITurnContext turnContext,
            IFixture fixture)
        {
            // Arrange
            var instrumentation = new BotInstrumentationMiddleware(this.telemetryClient, settings);
            var activity = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                ChannelId = fixture.Create<string>(),
            };
            Mock.Get(turnContext)
                .Setup(c => c.OnSendActivities(It.IsAny<SendActivitiesHandler>()))
                .Callback<SendActivitiesHandler>(h => h(null, new List<Activity> { activity }, () => Task.FromResult(Array.Empty<ResourceResponse>())));

            const int expectedNumberOfTelemetryProperties = 2;
            const string expectedTelemetryName = EventTypes.ConversationUpdate;

            // Act
            await instrumentation.OnTurnAsync(turnContext, null)
                .ConfigureAwait(false);

            // Assert
            this.mockTelemetryChannel.Verify(
                tc => tc.Send(It.Is<EventTelemetry>(t =>
                    t.Name == expectedTelemetryName &&
                    t.Properties.Count == expectedNumberOfTelemetryProperties &&
                    t.Properties[BotConstants.TypeProperty] == activity.Type &&
                    t.Properties[BotConstants.ChannelProperty] == activity.ChannelId)),
                Times.Once);
        }

        [Theory(DisplayName = "GIVEN turn context WHEN UpdateActivity is invoked THEN event telemetry is being sent")]
        [AutoMockData]
        public async void GIVENTurnContext_WHENUpdateActivityInvoked_THENEventTelemetryIsBeingSent(
            InstrumentationSettings settings,
            ITurnContext turnContext,
            IFixture fixture)
        {
            // Arrange
            var instrumentation = new BotInstrumentationMiddleware(this.telemetryClient, settings);
            var activity = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                ChannelId = fixture.Create<string>(),
            };
            Mock.Get(turnContext)
                .Setup(c => c.OnUpdateActivity(It.IsAny<UpdateActivityHandler>()))
                .Callback<UpdateActivityHandler>(h => h(null, activity, () =>
                    Task.FromResult((ResourceResponse)null)));
            const int expectedNumberOfTelemetryProperties = 2;
            const string expectedTelemetryName = EventTypes.ConversationUpdate;

            // Act
            await instrumentation.OnTurnAsync(turnContext, null)
                .ConfigureAwait(false);

            // Assert
            this.mockTelemetryChannel.Verify(
                tc => tc.Send(It.Is<EventTelemetry>(t =>
                    t.Name == expectedTelemetryName &&
                    t.Properties.Count == expectedNumberOfTelemetryProperties &&
                    t.Properties[BotConstants.TypeProperty] == activity.Type &&
                    t.Properties[BotConstants.ChannelProperty] == activity.ChannelId)),
                Times.Once);
        }

        [Theory(DisplayName = "GIVEN next turn WHEN OnTurnAsync is invoked THEN next turn is being invoked")]
        [AutoData]
        public async void GIVENNextTurn_WHENOnTurnAsyncIsInvoked_THENNextTurnIsBeingInvoked(
            InstrumentationSettings settings)
        {
            // Arrange
            var instrumentation = new BotInstrumentationMiddleware(this.telemetryClient, settings);
            var turnContext = new Mock<ITurnContext>();
            var nextTurnInvoked = false;

            // Act
            await instrumentation.OnTurnAsync(turnContext.Object, token => Task.Run(() => nextTurnInvoked = true, token))
                .ConfigureAwait(false);

            // Assert
            nextTurnInvoked.Should().Be(true);
        }

        [Theory(DisplayName = "GIVEN empty turn context WHEN OnTurnAsync is invoked THEN exception is being thrown")]
        [AutoData]
        public async void GIVENEmptyTurnContext_WHENOnTurnAsyncIsInvoked_THENExceptionIsBeingThrown(
            InstrumentationSettings settings)
        {
            // Arrange
            var instrumentation = new BotInstrumentationMiddleware(this.telemetryClient, settings);
            const ITurnContext emptyTurnContext = null;
            NextDelegate nextDelegate = Task.FromCanceled;

            // Act
            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => instrumentation.OnTurnAsync(emptyTurnContext, nextDelegate))
                .ConfigureAwait(false);
        }

        [Theory(DisplayName = "GIVEN empty telemetry client WHEN BotInstrumentationMiddleware is constructed THEN exception is being thrown")]
        [AutoData]
        public void GIVENEmptyTelemetryClient_WHENBotInstrumentationMiddlewareIsConstructed_THENExceptionIsBeingThrown(
            InstrumentationSettings settings)
        {
            // Arrange
            const TelemetryClient emptyTelemetryClient = null;

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => new BotInstrumentationMiddleware(emptyTelemetryClient, settings));
        }

        [Fact(DisplayName = "GIVEN empty settings WHEN BotInstrumentationMiddleware is constructed THEN exception is being thrown")]
        public void GIVENEmptySettings_WHENBotInstrumentationMiddlewareIsConstructed_THENExceptionIsBeingThrown()
        {
            // Arrange
            const InstrumentationSettings emptySettings = null;

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => new BotInstrumentationMiddleware(this.telemetryClient, emptySettings));
        }
    }
}
