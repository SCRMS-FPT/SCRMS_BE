using CourtBooking.Application.CourtManagement.Command.CreateCourtPromotion;
using CourtBooking.Application.DTOs;
using FluentValidation.TestHelper;
using System;
using Xunit;

namespace CourtBooking.Test.Application.Commands
{
    public class CreateCourtPromotionCommandTests
    {
        private readonly CreateCourtPromotionCommandValidator _validator;

        public CreateCourtPromotionCommandTests()
        {
            _validator = new CreateCourtPromotionCommandValidator();
        }

        [Fact]
        public void Constructor_Should_SetProperties_When_Called()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var courtId = Guid.NewGuid();

            // Act
            var command = new CreateCourtPromotionCommand(
                   courtId, "Discount for summer season", "Percentage", 20.0m, DateTime.Today, DateTime.Today.AddMonths(3), userId
            );

            // Assert
            Assert.Equal("Discount for summer season", command.Description);
            Assert.Equal(userId, command.UserId);
            Assert.Equal(courtId, command.CourtId);
        }

        [Fact]
        public void Validate_Should_Pass_When_AllPropertiesValid()
        {
            // Arrange
            var command = new CreateCourtPromotionCommand(
                   Guid.NewGuid(), "Discount for summer season", "Percentage", 20.0m, DateTime.Today, DateTime.Today.AddMonths(3), Guid.NewGuid()
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_Should_Pass_When_AllPropertiesValid_FixedAmount()
        {
            // Arrange
            var command = new CreateCourtPromotionCommand(
                 Guid.NewGuid(),
                     "Fixed discount for weekends",
                     "FixedAmount",
                     50000.0m, // Can be any positive value for fixed amount
                     DateTime.Today,
                     DateTime.Today.AddMonths(3),
                Guid.NewGuid()
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}