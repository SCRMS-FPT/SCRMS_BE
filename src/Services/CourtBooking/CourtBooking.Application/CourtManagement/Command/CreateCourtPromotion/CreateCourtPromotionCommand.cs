using CourtBooking.Application.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.CourtManagement.Command.CreateCourtPromotion
{
    public record CreateCourtPromotionCommand(
           Guid CourtId,
           string Description,
           string DiscountType,
           decimal DiscountValue,
           DateTime ValidFrom,
           DateTime ValidTo,
           Guid UserId) : IRequest<CourtPromotionDTO>;

    public class CreateCourtPromotionCommandValidator : AbstractValidator<CreateCourtPromotionCommand>
    {
        public CreateCourtPromotionCommandValidator()
        {
            RuleFor(c => c.CourtId).NotEqual(Guid.Empty)
                .WithMessage("ID của sân không được để trống");

            RuleFor(c => c.Description).NotEmpty()
                .WithMessage("Tên khuyến mãi không được để trống")
                .MaximumLength(100)
                .WithMessage("Tên khuyến mãi không được vượt quá 100 ký tự");

            RuleFor(c => c.DiscountValue).GreaterThan(0)
                .WithMessage("Giá trị khuyến mãi phải lớn hơn 0");

            RuleFor(c => c.DiscountType).NotEmpty()
                .WithMessage("Loại khuyến mãi không được để trống")
                .Must(type => type == "Percentage" || type == "FixedAmount")
                .WithMessage("Loại khuyến mãi phải là 'Percentage' hoặc 'FixedAmount'");

            RuleFor(c => c)
                .Must(dto => dto.ValidTo > dto.ValidFrom)
                .WithMessage("Ngày kết thúc phải sau ngày bắt đầu");

            RuleFor(c => c)
                .Must(dto => dto.DiscountType != "Percentage" || dto.DiscountValue <= 100)
                .WithMessage("Giá trị khuyến mãi theo phần trăm không được vượt quá 100%");

            RuleFor(c => c.UserId).NotEqual(Guid.Empty)
                .WithMessage("ID người dùng không được để trống");
        }
    }
}