using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using CourtBooking.Domain.Exceptions;
using System;
using System.Collections.Generic;
using Xunit;
using CourtBooking.Domain.Enums;

namespace CourtBooking.Test.Domain.Models
{
    public class SportCenterTests
    {
        [Fact]
        public void Create_Should_ReturnValidSportCenter_WhenParametersAreValid()
        {
            // Arrange
            var id = SportCenterId.Of(Guid.NewGuid());
            var ownerId = OwnerId.Of(Guid.NewGuid());
            var name = "Tennis Center";
            var phoneNumber = "0123456789";
            var address = new Location("123 Main St", "HCMC", "Vietnam", "70000");
            var location = new GeoLocation(10.762622, 106.660172);
            var images = new SportCenterImages("main.jpg", new List<string> { "1.jpg", "2.jpg" });
            var description = "A great tennis center";

            // Act
            var sportCenter = SportCenter.Create(id, ownerId, name, phoneNumber, address, location, images, description);

            // Assert
            Assert.Equal(id, sportCenter.Id);
            Assert.Equal(ownerId, sportCenter.OwnerId);
            Assert.Equal(name, sportCenter.Name);
            Assert.Equal(phoneNumber, sportCenter.PhoneNumber);
            Assert.Equal(address, sportCenter.Address);
            Assert.Equal(location, sportCenter.LocationPoint);
            Assert.Equal(images, sportCenter.Images);
            Assert.Equal(description, sportCenter.Description);
        }

        [Fact]
        public void Create_Should_AcceptEmptyDescription_AndEmptyImages()
        {
            // Arrange
            var id = SportCenterId.Of(Guid.NewGuid());
            var ownerId = OwnerId.Of(Guid.NewGuid());
            var name = "Tennis Center";
            var phoneNumber = "0123456789";
            var address = new Location("123 Main St", "HCMC", "Vietnam", "70000");
            var location = new GeoLocation(10.762622, 106.660172);
            var images = new SportCenterImages("default-avatar.jpg", new List<string>());

            // Act
            var sportCenter = SportCenter.Create(id, ownerId, name, phoneNumber, address, location, images, null);

            // Assert
            Assert.Equal(string.Empty, sportCenter.Description);
            Assert.NotNull(sportCenter.Images);
            Assert.Equal("default-avatar.jpg", sportCenter.Images.Avatar);
            Assert.Empty(sportCenter.Images.ImageUrls);
        }

        [Fact]
        public void UpdateInfo_Should_UpdateNamePhoneAndDescription_AndSetLastModified()
        {
            // Arrange
            var sportCenter = CreateValidSportCenter();
            var newName = "Updated Tennis Center";
            var newPhone = "9876543210";
            var newDescription = "Updated description";

            // Act
            sportCenter.UpdateInfo(newName, newPhone, newDescription);

            // Assert
            Assert.Equal(newName, sportCenter.Name);
            Assert.Equal(newPhone, sportCenter.PhoneNumber);
            Assert.Equal(newDescription, sportCenter.Description);
            Assert.NotNull(sportCenter.LastModified);
        }

        [Theory]
        [InlineData(null, "0123456789", "Description")]
        [InlineData("", "0123456789", "Description")]
        [InlineData("  ", "0123456789", "Description")]
        [InlineData("Tennis Center", null, "Description")]
        [InlineData("Tennis Center", "", "Description")]
        [InlineData("Tennis Center", "  ", "Description")]
        public void UpdateInfo_Should_ThrowDomainException_WhenNameOrPhoneIsInvalid(string name, string phone, string description)
        {
            // Arrange
            var sportCenter = CreateValidSportCenter();

            // Act & Assert
            var exception = Assert.Throws<DomainException>(() => sportCenter.UpdateInfo(name, phone, description));
            if (string.IsNullOrWhiteSpace(name))
                Assert.Contains("Name is required", exception.Message);
            else
                Assert.Contains("Phone number is required", exception.Message);
        }

        [Fact]
        public void ChangeLocation_Should_UpdateAddressAndLocationPoint_AndSetLastModified()
        {
            // Arrange
            var sportCenter = CreateValidSportCenter();
            var newAddress = new Location("456 New St", "HCMC", "Vietnam", "70000");
            var newLocation = new GeoLocation(10.8, 106.7);

            // Act
            sportCenter.ChangeLocation(newAddress, newLocation);

            // Assert
            Assert.Equal(newAddress, sportCenter.Address);
            Assert.Equal(newLocation, sportCenter.LocationPoint);
            Assert.NotNull(sportCenter.LastModified);
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData(true, null)]
        public void ChangeLocation_Should_ThrowDomainException_WhenAddressOrLocationIsNull(bool nullAddress, bool nullLocation)
        {
            // Arrange
            var sportCenter = CreateValidSportCenter();
            var address = nullAddress ? null : new Location("456 New St", "HCMC", "Vietnam", "70000");
            var location = nullLocation ? null : new GeoLocation(10.8, 106.7);

            // Act & Assert
            var exception = Assert.Throws<DomainException>(() => sportCenter.ChangeLocation(address, location));
            if (nullAddress)
                Assert.Contains("New address is required", exception.Message);
            else
                Assert.Contains("New location is required", exception.Message);
        }

        [Fact]
        public void ChangeImages_Should_UpdateImages_AndSetLastModified()
        {
            // Arrange
            var sportCenter = CreateValidSportCenter();
            var newImages = new SportCenterImages("new-main.jpg", new List<string> { "new1.jpg", "new2.jpg" });

            // Act
            sportCenter.ChangeImages(newImages);

            // Assert
            Assert.Equal(newImages, sportCenter.Images);
            Assert.NotNull(sportCenter.LastModified);
        }

        [Fact]
        public void ChangeImages_Should_ThrowDomainException_WhenImagesIsNull()
        {
            // Arrange
            var sportCenter = CreateValidSportCenter();

            // Act & Assert
            var exception = Assert.Throws<DomainException>(() => sportCenter.ChangeImages(null));
            Assert.Contains("Images cannot be null", exception.Message);
        }

        [Fact]
        public void AddCourt_Should_AddCourtToCollection()
        {
            // Arrange
            var sportCenter = CreateValidSportCenter();
            var court = CreateCourt(sportCenter.Id);

            // Act
            sportCenter.AddCourt(court);

            // Assert
            Assert.Single(sportCenter.Courts);
            Assert.Contains(court, sportCenter.Courts);
        }

        private SportCenter CreateValidSportCenter()
        {
            return SportCenter.Create(
                SportCenterId.Of(Guid.NewGuid()),
                OwnerId.Of(Guid.NewGuid()),
                "Tennis Center",
                "0123456789",
                new Location("123 Main St", "HCMC", "Vietnam", "70000"),
                new GeoLocation(10.762622, 106.660172),
                new SportCenterImages("main.jpg", new List<string> { "1.jpg", "2.jpg" }),
                "A great tennis center"
            );
        }

        private Court CreateCourt(SportCenterId sportCenterId)
        {
            return Court.Create(
                CourtId.Of(Guid.NewGuid()),
                CourtName.Of("Tennis Court 1"),
                sportCenterId,
                SportId.Of(Guid.NewGuid()),
                TimeSpan.FromMinutes(60),
                "Description",
                "facilities",
                CourtType.Indoor,
                50
            );
        }
    }
}