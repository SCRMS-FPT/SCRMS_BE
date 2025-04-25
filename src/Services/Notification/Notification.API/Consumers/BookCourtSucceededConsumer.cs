//using MassTransit;
//using Microsoft.AspNetCore.SignalR;
//using Notification.API.Entities;
//using Notification.API.Hubs;
//using Notification.API.Services;
//using System;
//using System.Threading.Tasks;

//namespace Notification.API.Consumers
//{
//    public class BookCourtSucceededConsumer : IConsumer<BookCourtSucceededEvent>
//    {
//        private readonly INotificationService _notificationService;
//        private readonly IHubContext<NotifyHub> _hubContext;

//        public BookCourtSucceededConsumer(INotificationService notificationService, IHubContext<NotifyHub> hubContext)
//        {
//            _notificationService = notificationService;
//            _hubContext = hubContext;
//        }

//        public async Task Consume(ConsumeContext<BookCourtSucceededEvent> context)
//        {
//            var bookingEvent = context.Message;

//            // Log that we received the event
//            Console.WriteLine($"Received BookCourtSucceededEvent: TransactionId={bookingEvent.TransactionId}, UserId={bookingEvent.UserId}");

//            // Tạo thông báo
//            var noti = new MessageNotification
//            {
//                Receiver = bookingEvent.UserId,
//                Title = "Đặt sân thành công",
//                Content = $"Bạn đã đặt sân thành công với số tiền {bookingEvent.Amount:N0} VND. {bookingEvent.Description}",
//                Type = "booking",
//                IsRead = false
//            };

//            // Lưu thông báo vào database
//            await _notificationService.SaveNotification(noti);

//            // Gửi thông báo qua SignalR hub nếu người dùng đang kết nối
//            await _hubContext.Clients.User(bookingEvent.UserId.ToString())
//                .SendAsync("ReceiveNotification", noti);
//        }
//    }
//}