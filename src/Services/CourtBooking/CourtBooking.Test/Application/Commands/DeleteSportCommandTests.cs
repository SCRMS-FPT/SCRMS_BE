using CourtBooking.Application.SportManagement.Commands.DeleteSport;
using FluentValidation.TestHelper;
using System;
using Xunit;

namespace CourtBooking.Test.Application.Commands
{
    public class DeleteSportCommandTests
    {
        private readonly DeleteSportCommandValidator _validator;

        public DeleteSportCommandTests()
        {
            _validator = new DeleteSportCommandValidator();
        }

        [Fact]
        public void Constructor_Should_SetSportId_When_Called()
        {
            // Arrange
            var sportId = Guid.NewGuid();
            
            // Act
            var command = new DeleteSportCommand(sportId);
            
            // Assert
            Assert.Equal(sportId, command.SportId);
        }

        [Fact]
        public void Validate_Should_Fail_When_SportIdIsEmpty()
        {
            // Arrange
            var command = new DeleteSportCommand(Guid.Empty);
            
            // Act
            var result = _validator.TestValidate(command);
            
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SportId);
        }

        [Fact]
        public void Validate_Should_Pass_When_SportIdIsValid()
        {
            // Arrange
            var command = new DeleteSportCommand(Guid.NewGuid());
            
            // Act
            var result = _validator.TestValidate(command);
            
            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
} 