using System;
using Xunit;
using FluentAssertions;
using Identity.Domain.Models;

namespace Identity.Test.Domain.Models
{
    public class ServicePackageTests
    {
        [Fact]
        public void Create_ShouldReturnServicePackage_WhenInputIsValid()
        {
            string name = "Test Package"; string description = "Test Description"; decimal price = 100.00m; int duration = 30; string associatedRole = "Basic";
            // Act
            var package = ServicePackage.Create(name, description, price, duration, associatedRole);

            // Assert
            package.Should().NotBeNull();
            package.Name.Should().Be(name);
            package.Description.Should().Be(description);
            package.Price.Should().Be(price);
            package.DurationDays.Should().Be(duration);
            package.AssociatedRole.Should().Be(associatedRole);
        }

        [Fact]
        public void Create_ShouldReturnServicePackage_WhenNameIsAtMaxLength()
        {
            // Arrange
            string name = new string('X', 255);
            // Act
            var package = ServicePackage.Create(name, "Desc", 100.00m, 30, "Basic");
            // Assert
            package.Name.Length.Should().Be(255);
        }

        [Fact]
        public void Create_ShouldThrowException_WhenPriceIsNonPositive()
        {
            // Arrange & Act
            Action act = () => ServicePackage.Create("Name", "Desc", 0, 30, "Basic");

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("Price must be positive");
        }

        [Fact]
        public void UpdateDetails_ShouldUpdatePropertiesCorrectly()
        {
            // Arrange
            var package = ServicePackage.Create("Old", "Old Desc", 100, 30, "Basic");

            // Act
            package.UpdateDetails("New", "New Desc", 150, 45, "Premium", "active");

            // Assert
            package.Name.Should().Be("New");
            package.Description.Should().Be("New Desc");
            package.Price.Should().Be(150);
            package.DurationDays.Should().Be(45);
            package.AssociatedRole.Should().Be("Premium");
            package.Status.Should().Be("active");
        }
    }
}