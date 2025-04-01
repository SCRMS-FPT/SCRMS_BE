using BuildingBlocks.Messaging.Events;
using Coach.API.Data.Models;
using Coach.API.Data.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Consumers
{
    public class CoachPaymentConsumer : IConsumer<PaymentSucceededEvent>, IConsumer<CoachPaymentEvent>
    {
        private readonly ILogger<CoachPaymentConsumer> _logger;
        private readonly ICoachBookingRepository _bookingRepository;
        private readonly ICoachPackagePurchaseRepository _packagePurchaseRepository;
        private readonly ICoachPackageRepository _packageRepository;

        public CoachPaymentConsumer(
            ILogger<CoachPaymentConsumer> logger,
            ICoachBookingRepository bookingRepository,
            ICoachPackagePurchaseRepository packagePurchaseRepository,
            ICoachPackageRepository packageRepository)
        {
            _logger = logger;
            _bookingRepository = bookingRepository;
            _packagePurchaseRepository = packagePurchaseRepository;
            _packageRepository = packageRepository;
        }

        // Xử lý event thanh toán cũ
        public async Task Consume(ConsumeContext<PaymentSucceededEvent> context)
        {
            var paymentEvent = context.Message;

            // Chỉ xử lý nếu loại thanh toán liên quan đến Coach service
            if (paymentEvent.PaymentType == "CoachBooking" ||
                paymentEvent.PaymentType == "CoachPackage" ||
                paymentEvent.PaymentType.StartsWith("Coach"))
            {
                _logger.LogInformation("Xử lý thanh toán cho Coach: {TransactionId}, PaymentType: {PaymentType}",
                    paymentEvent.TransactionId, paymentEvent.PaymentType);

                // Thực hiện xử lý thanh toán cho coach
                await ProcessCoachPayment(paymentEvent);
            }
            else
            {
                _logger.LogDebug("Bỏ qua sự kiện thanh toán không phải dành cho Coach service: {PaymentType}",
                    paymentEvent.PaymentType);
            }
        }

        // Xử lý event chuyên biệt cho thanh toán coach
        public async Task Consume(ConsumeContext<CoachPaymentEvent> context)
        {
            var paymentEvent = context.Message;

            _logger.LogInformation("Xử lý thanh toán coach: {TransactionId}, CoachId: {CoachId}",
                paymentEvent.TransactionId, paymentEvent.CoachId);

            try
            {
                // Xử lý thanh toán dựa vào BookingId hoặc PackageId
                if (paymentEvent.BookingId.HasValue)
                {
                    // Thanh toán cho việc đặt lịch coach
                    await ProcessCoachBookingPayment(paymentEvent);
                }
                else if (paymentEvent.PackageId.HasValue)
                {
                    // Thanh toán mua gói huấn luyện
                    await ProcessCoachPackagePayment(paymentEvent);
                }
                else
                {
                    _logger.LogWarning("Không xác định được loại thanh toán: {TransactionId}",
                        paymentEvent.TransactionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý thanh toán coach: {TransactionId}",
                    paymentEvent.TransactionId);
                // Consider implementing retry or error handling mechanism
            }
        }

        private async Task ProcessCoachPayment(PaymentBaseEvent payment)
        {
            // Xử lý thanh toán cơ bản cho event cũ
            _logger.LogInformation("Xử lý thanh toán cơ bản: {TransactionId}", payment.TransactionId);

            // Kiểm tra nếu là thanh toán mua gói coach
            if (payment is PaymentSucceededEvent successEvent &&
                successEvent.PaymentType == "CoachPackage" &&
                successEvent.ReferenceId.HasValue)
            {
                await ProcessCoachPackagePaymentFromGenericEvent(successEvent);
            }
        }

        private async Task ProcessCoachPackagePaymentFromGenericEvent(PaymentSucceededEvent payment)
        {
            if (!payment.ReferenceId.HasValue)
            {
                _logger.LogWarning("Không có ReferenceId trong sự kiện thanh toán gói: {TransactionId}",
                    payment.TransactionId);
                return;
            }

            var packageId = payment.ReferenceId.Value;
            _logger.LogInformation("Xử lý mua gói huấn luyện từ PaymentSucceededEvent: {PackageId} cho người dùng {UserId}",
                packageId, payment.UserId);

            // Lấy thông tin gói coach
            var package = await _packageRepository.GetCoachPackageByIdAsync(packageId, CancellationToken.None);
            if (package == null)
            {
                _logger.LogWarning("Không tìm thấy gói huấn luyện với ID: {PackageId}", packageId);
                return;
            }

            // Tạo mới CoachPackagePurchase
            var packagePurchase = new CoachPackagePurchase
            {
                Id = Guid.NewGuid(),
                UserId = payment.UserId,
                CoachPackageId = packageId,
                PurchaseDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(3), // Giả sử gói có hiệu lực 3 tháng
                SessionsUsed = 0, // Số buổi đã sử dụng ban đầu là 0
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Lưu vào database
            await _packagePurchaseRepository.AddCoachPackagePurchaseAsync(packagePurchase, CancellationToken.None);

            _logger.LogInformation("Đã tạo gói huấn luyện cho người dùng từ PaymentSucceededEvent: UserID {UserId}, PackageID {PackageId}",
                payment.UserId, packageId);
        }

        private async Task ProcessCoachBookingPayment(CoachPaymentEvent payment)
        {
            if (!payment.BookingId.HasValue)
            {
                _logger.LogWarning("Không có BookingId trong sự kiện thanh toán: {TransactionId}",
                    payment.TransactionId);
                return;
            }

            var bookingId = payment.BookingId.Value;
            _logger.LogInformation("Cập nhật trạng thái đặt lịch coach: {BookingId}", bookingId);

            // Tìm booking cần cập nhật
            var booking = await _bookingRepository.GetCoachBookingByIdAsync(bookingId, CancellationToken.None);

            if (booking == null)
            {
                _logger.LogWarning("Không tìm thấy lịch đặt coach với ID: {BookingId}", bookingId);
                return;
            }

            // Kiểm tra xem booking có thuộc về người dùng này không
            if (booking.UserId != payment.UserId)
            {
                _logger.LogWarning("Lịch đặt coach {BookingId} không thuộc về người dùng {UserId}",
                    bookingId, payment.UserId);
                return;
            }

            // Cập nhật trạng thái booking thành "completed"
            booking.Status = "completed";

            // Lưu vào database
            await _bookingRepository.UpdateCoachBookingAsync(booking, CancellationToken.None);

            _logger.LogInformation("Đã cập nhật trạng thái đặt lịch coach thành completed: {BookingId}",
                bookingId);
        }

        private async Task ProcessCoachPackagePayment(CoachPaymentEvent payment)
        {
            if (!payment.PackageId.HasValue)
            {
                _logger.LogWarning("Không có PackageId trong sự kiện thanh toán: {TransactionId}",
                    payment.TransactionId);
                return;
            }

            var packageId = payment.PackageId.Value;
            _logger.LogInformation("Xử lý mua gói huấn luyện: {PackageId} cho người dùng {UserId}",
                packageId, payment.UserId);

            // Kiểm tra xem gói huấn luyện có tồn tại không
            var package = await _packageRepository.GetCoachPackageByIdAsync(packageId, CancellationToken.None);
            if (package == null)
            {
                _logger.LogWarning("Không tìm thấy gói huấn luyện với ID: {PackageId}", packageId);
                return;
            }

            // Kiểm tra xem gói huấn luyện có thuộc về coach được chỉ định không
            if (package.CoachId != payment.CoachId)
            {
                _logger.LogWarning("Gói huấn luyện {PackageId} không thuộc về coach {CoachId}",
                    packageId, payment.CoachId);
                return;
            }

            // Tạo mới CoachPackagePurchase - sửa các thuộc tính theo đúng model
            var packagePurchase = new CoachPackagePurchase
            {
                Id = Guid.NewGuid(),
                UserId = payment.UserId,
                CoachPackageId = packageId, // Sửa từ PackageId sang CoachPackageId
                PurchaseDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(3), // Giả sử gói có hiệu lực 3 tháng
                SessionsUsed = 0, // Số buổi đã sử dụng ban đầu là 0
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
                // Bỏ các thuộc tính không tồn tại trong model
            };

            // Lưu vào database - sử dụng AddCoachPackagePurchaseAsync thay vì CreateCoachPackagePurchaseAsync
            await _packagePurchaseRepository.AddCoachPackagePurchaseAsync(packagePurchase, CancellationToken.None);

            _logger.LogInformation("Đã tạo gói huấn luyện cho người dùng: UserID {UserId}, PackageID {PackageId}",
                payment.UserId, packageId);
        }
    }
}