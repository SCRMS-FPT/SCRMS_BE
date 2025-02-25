using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Identity.Test.Domain.Models
{
    public class ServicePackageTests
    {
        [Fact]
        public void Create_ShouldReturnServicePackage_WhenInputIsValid()
        { // Arrange
            string name = "Test Package"; string description = "Test Description"; decimal price = 100; int duration = 30; string associatedRole = "Basic";

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

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void Create_ShouldThrowException_WhenPriceIsNonPositive(decimal price)
        {
            // Arrange
            Action act = () => ServicePackage.Create("Name", "Desc", price, 30, "Basic");

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("Price must be positive");
        }

        [Fact]
        public void UpdateDetails_ShouldUpdatePropertiesCorrectly()
        {
            // Arrange
            var package = ServicePackage.Create("Old", "Old Desc", 100, 30, "Basic");

            // Act
            package.UpdateDetails("New", "New Desc", 150, 45, "Premium");

            // Assert
            package.Name.Should().Be("New");
            package.Description.Should().Be("New Desc");
            package.Price.Should().Be(150);
            package.DurationDays.Should().Be(45);
            package.AssociatedRole.Should().Be("Premium");
        }
    }
}