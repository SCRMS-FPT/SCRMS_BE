using BuildingBlocks.Messaging.Events;
using MassTransit;
using Notification.API.Services;

namespace Notification.API.Consumers;

public class MatchCreatedConsumer : IConsumer<MatchCreatedEvent>
{
    private readonly INotificationService _notificationService;

    public MatchCreatedConsumer(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Consume(ConsumeContext<MatchCreatedEvent> context)
    {
        var message = context.Message;
        await _notificationService.SendPushNotification(
            message.UserId1,
            "New Match!",
            $"You have matched with {message.UserId2}");

        await _notificationService.SendPushNotification(
            message.UserId2,
            "New Match!",
            $"You have matched with {message.UserId1}");
    }
}