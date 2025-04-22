using FluentValidation;
using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Enums;

namespace CourtBooking.Application.CourtManagement.Command.CreateCourt
{
    public class CreateCourtCommandValidator : AbstractValidator<CreateCourtCommand>
    {
        public CreateCourtCommandValidator()
        {
            RuleFor(c => c.Court).NotNull()
                .WithMessage("Thông tin sân không được để trống");

            When(c => c.Court != null, () =>
            {
                RuleFor(c => c.Court.CourtName)
                    .NotEmpty().WithMessage("Tên sân không được để trống")
                    .MaximumLength(100).WithMessage("Tên sân không được vượt quá 100 ký tự");

                RuleFor(c => c.Court.SportCenterId)
                    .NotEqual(Guid.Empty).WithMessage("ID Trung tâm thể thao không được để trống");

                RuleFor(c => c.Court.SportId)
                    .NotEqual(Guid.Empty).WithMessage("ID Môn thể thao không được để trống");

                RuleFor(c => c.Court.SlotDuration)
                    .Must(duration => duration.TotalMinutes >= 30 && duration.TotalMinutes <= 240)
                    .WithMessage("Thời lượng mỗi khung giờ phải từ 30 phút đến 4 giờ");

                RuleFor(c => c.Court.MinDepositPercentage)
                    .InclusiveBetween(0, 100)
                    .WithMessage("Phần trăm đặt cọc tối thiểu phải từ 0% đến 100%");

                RuleFor(c => c.Court.CourtType)
                    .Must(courtType => Enum.IsDefined(typeof(CourtType), courtType))
                    .WithMessage("Loại sân không hợp lệ");

                RuleFor(c => c.Court.CancellationWindowHours)
                    .GreaterThanOrEqualTo(1)
                    .WithMessage("Thời gian cho phép hủy sân phải ít nhất 1 giờ");

                RuleFor(c => c.Court.RefundPercentage)
                    .InclusiveBetween(0, 100)
                    .WithMessage("Phần trăm hoàn tiền phải từ 0% đến 100%");

                RuleFor(c => c.Court.CourtSchedules)
                    .NotEmpty()
                    .WithMessage("Lịch hoạt động của sân không được trống");
            });
        }
    }
}