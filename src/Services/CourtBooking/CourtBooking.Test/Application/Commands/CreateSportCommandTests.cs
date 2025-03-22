using CourtBooking.Application.SportManagement.Commands.CreateSport;
using FluentValidation.TestHelper;
using System;
using Xunit;

namespace CourtBooking.Test.Application.Commands
{
    public class CreateSportCommandTests
    {
        private readonly CreateSportCommandValidator _validator;

        public CreateSportCommandTests()
        {
            _validator = new CreateSportCommandValidator();
        }

        [Fact]
        public void Constructor_Should_SetProperties_When_Called()
        {
            // Arrange
            var name = "Tennis";
            var description = "Tennis is a racket sport";
            var icon = "tennis.png";
            
            // Act
            var command = new CreateSportCommand(name, description, icon);
            
            // Assert
            Assert.Equal(name, command.Name);
            Assert.Equal(description, command.Description);
            Assert.Equal(icon, command.Icon);
        }

        [Fact]
        public void Validate_Should_Fail_When_NameIsEmpty()
        {
            // Arrange
            var command = new CreateSportCommand("", "Description", "icon.png");
            
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
            var command = new CreateSportCommand(longName, "Description", "icon.png");
            
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
            var command = new CreateSportCommand("Tennis", longDescription, "icon.png");
            
            // Act
            var result = _validator.TestValidate(command);
            
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Validate_Should_Fail_When_IconIsEmpty()
        {
            // Arrange
            var command = new CreateSportCommand("Tennis", "Description", "");
            
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
            var command = new CreateSportCommand("Tennis", "Description", longIcon);
            
            // Act
            var result = _validator.TestValidate(command);
            
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Icon);
        }

        [Fact]
        public void Validate_Should_Pass_When_AllPropertiesValid()
        {
            // Arrange
            var command = new CreateSportCommand("Tennis", "Tennis is a racket sport", "tennis.png");
            
            // Act
            var result = _validator.TestValidate(command);
            
            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
} 