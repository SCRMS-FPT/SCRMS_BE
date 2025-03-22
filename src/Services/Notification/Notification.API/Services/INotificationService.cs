﻿namespace Notification.API.Services
{
    public interface INotificationService
    {
        Task SendPaymentConfirmation(
            Guid userId,
            decimal amount,
            string description);

        Task SendRefundNotification(
            Guid userId,
            decimal amount,
            string reason);

        Task SendPushNotification(
            Guid userId,
            string title,
            string message,
            Dictionary<string, string>? data = null);

        Task SendToOwner(
            Guid ownerId,
            string message,
            Dictionary<string, string>? data = null);
    }
}