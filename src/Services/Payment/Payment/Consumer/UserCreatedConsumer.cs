//using BuildingBlocks.Abstractions.Messaging;
using Identity.Domain.Events;
using MassTransit;
using Payment.API.Data.Repositories;

namespace Payment.API.Consumer
{
    public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
    {
        private readonly IUserWalletRepository _userWalletRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserCreatedConsumer> _logger;

        public UserCreatedConsumer(
            IUserWalletRepository userWalletRepository,
            IUnitOfWork unitOfWork,
            ILogger<UserCreatedConsumer> logger)
        {
            _userWalletRepository = userWalletRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserCreatedEvent> context)
        {
            var userCreatedEvent = context.Message;

            _logger.LogInformation(
                "Đã nhận UserCreatedEvent: UserId={UserId}",
                userCreatedEvent.UserId);

            // Kiểm tra xem người dùng đã có ví chưa
            var existingWallet = await _userWalletRepository.GetUserWalletByUserIdAsync(
                userCreatedEvent.UserId,
                context.CancellationToken);

            if (existingWallet != null)
            {
                _logger.LogInformation(
                    "Người dùng {UserId} đã có ví, bỏ qua việc tạo ví mới",
                    userCreatedEvent.UserId);
                return;
            }

            // Tạo ví mới cho người dùng với số dư ban đầu là 0
            var newWallet = new UserWallet
            {
                UserId = userCreatedEvent.UserId,
                Balance = 0,
                UpdatedAt = DateTime.UtcNow
            };

            await _userWalletRepository.AddUserWalletAsync(newWallet, context.CancellationToken);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation(
                "Đã tạo ví mới cho người dùng {UserId}",
                userCreatedEvent.UserId);
        }
    }
}