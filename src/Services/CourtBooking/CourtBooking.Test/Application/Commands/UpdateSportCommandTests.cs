using CourtBooking.Application.SportManagement.Commands.UpdateSport;
using FluentValidation.TestHelper;
using System;
using Xunit;

namespace CourtBooking.Test.Application.Commands
{
    public class UpdateSportCommandTests
    {
        private readonly UpdateSportCommandValidator _validator;

        public UpdateSportCommandTests()
        {
            _validator = new UpdateSportCommandValidator();
        }

        [Fact]
        public void Constructor_Should_SetProperties_When_Called()
        {
            // Arrange
            var id = Guid.NewGuid();
            var name = "Tennis";
            var description = "Tennis is a racket sport";
            var icon = "tennis.png";

            // Act
            var command = new UpdateSportCommand(id, name, description, icon);

            // Assert
            Assert.Equal(id, command.Id);
            Assert.Equal(name, command.Name);
            Assert.Equal(description, command.Description);
            Assert.Equal(icon, command.Icon);
        }

        [Fact]
        public void Validate_Should_Fail_When_IdIsEmpty()
        {
            // Arrange
            var command = new UpdateSportCommand(Guid.Empty, "Tennis", "Description", "icon.png");

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public void Validate_Should_Fail_When_NameIsEmpty()
        {
            // Arrange
            var command = new UpdateSportCommand(Guid.NewGuid(), "", "Description", "icon.png");

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Validate_Should_Fail_When_NameIsTooLong()
        {
            // Arrange
            var longName = new string('a', 101); // 101 characters
            var command = new UpdateSportCommand(Guid.NewGuid(), longName, "Description", "icon.png");

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Validate_Should_Fail_When_DescriptionIsTooLong()
        {
            // Arrange
            var longDescription = new string('a', 501); // 501 characters
            var command = new UpdateSportCommand(Guid.NewGuid(), "Tennis", longDescription, "icon.png");

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Validate_Should_Fail_When_IconIsEmpty()
        {
            // Arrange
            var command = new UpdateSportCommand(Guid.NewGuid(), "Tennis", "Description", "");

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Icon);
        }

        [Fact]
        public void Validate_Should_Fail_When_IconIsTooLong()
        {
            // Arrange
            var longIcon = new string('a', 201); // 201 characters
            var command = new UpdateSportCommand(Guid.NewGuid(), "Tennis", "Description", longIcon);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Icon);
        }

        [Fact]
        public void Validate_Should_Pass_When_AllPropertiesValid()
        {
            // Arrange
            var command = new UpdateSportCommand(Guid.NewGuid(), "Tennis", "Tennis is a racket sport", "tennis.png");

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}