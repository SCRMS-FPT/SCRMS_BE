using CourtBooking.Domain.Exceptions;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using CourtBooking.Domain.Enums;
using System;
using Xunit;

namespace CourtBooking.Test.Domain.Models
{
    public class CourtTests
    {
        [Fact]
        public void Create_Should_ReturnValidCourt_When_InputIsValid()
        {
            // Arrange
            var courtId = CourtId.Of(Guid.NewGuid());
            var sportCenterId = SportCenterId.Of(Guid.NewGuid());
            var sportId = SportId.Of(Guid.NewGuid());
            var courtName = CourtName.Of("Tennis Court 1");
            var description = "Main tennis court";
            var facilities = "Lights, Water";
            var slotDuration = TimeSpan.FromMinutes(60);
            var courtType = CourtType.Indoor;
            var minDepositPercentage = 50m;

            // Act
            var court = Court.Create(
                courtId,
                courtName,
                sportCenterId,
                sportId,
                slotDuration,
                description,
                facilities,
                courtType,
                minDepositPercentage
            );

            // Assert
            Assert.Equal(courtId, court.Id);
            Assert.Equal(sportCenterId, court.SportCenterId);
            Assert.Equal(sportId, court.SportId);
            Assert.Equal(courtName, court.CourtName);
            Assert.Equal(description, court.Description);
            Assert.Equal(facilities, court.Facilities);
            Assert.Equal(slotDuration, court.SlotDuration);
            Assert.Equal(courtType, court.CourtType);
            Assert.Equal(minDepositPercentage, court.MinDepositPercentage);
            Assert.Equal(CourtStatus.Open, court.Status);
        }

        [Fact]
        public void Create_Should_ThrowException_When_DepositPercentageInvalid()
        {
            // Arrange
            var courtId = CourtId.Of(Guid.NewGuid());
            var sportCenterId = SportCenterId.Of(Guid.NewGuid());
            var sportId = SportId.Of(Guid.NewGuid());
            var courtName = CourtName.Of("Tennis Court 1");
            var description = "Main tennis court";
            var facilities = "Lights, Water";
            var slotDuration = TimeSpan.FromMinutes(60);
            var courtType = CourtType.Indoor;
            var invalidDepositPercentage = 120m; // Trên 100%

            // Act & Assert
            Assert.Throws<DomainException>(() =>
                Court.Create(
                    courtId,
                    courtName,
                    sportCenterId,
                    sportId,
                    slotDuration,
                    description,
                    facilities,
                    courtType,
                    invalidDepositPercentage
                ));

            // Test với giá trị âm
            Assert.Throws<DomainException>(() =>
                Court.Create(
                    courtId,
                    courtName,
                    sportCenterId,
                    sportId,
                    slotDuration,
                    description,
                    facilities,
                    courtType,
                    -10m
                ));
        }

        [Fact]
        public void UpdateCourt_Should_UpdateProperties_When_Called()
        {
            // Arrange
            var court = CreateValidCourt();
            var newCourtName = CourtName.Of("Updated Name");
            var newSportId = SportId.Of(Guid.NewGuid());
            var newSlotDuration = TimeSpan.FromMinutes(90);
            var newDescription = "Updated Description";
            var newFacilities = "Updated Facilities";
            var newStatus = CourtStatus.Closed;
            var newCourtType = CourtType.Outdoor;
            var newMinDepositPercentage = 75m;
            var newCancellationWindowHours = 36;
            var newRefundPercentage = 50m;

            // Act
            court.UpdateCourt(
                newCourtName,
                newSportId,
                newSlotDuration,
                newDescription,
                newFacilities,
                newStatus,
                newCourtType,
                newMinDepositPercentage,
                newCancellationWindowHours,
                newRefundPercentage
            );

            // Assert
            Assert.Equal(newCourtName, court.CourtName);
            Assert.Equal(newSportId, court.SportId);
            Assert.Equal(newSlotDuration, court.SlotDuration);
            Assert.Equal(newDescription, court.Description);
            Assert.Equal(newFacilities, court.Facilities);
            Assert.Equal(newStatus, court.Status);
            Assert.Equal(newCourtType, court.CourtType);
            Assert.Equal(newMinDepositPercentage, court.MinDepositPercentage);
            Assert.Equal(newCancellationWindowHours, court.CancellationWindowHours);
            Assert.Equal(newRefundPercentage, court.RefundPercentage);
        }

        [Fact]
        public void UpdateCourt_Should_ThrowException_When_DepositPercentageInvalid()
        {
            // Arrange
            var court = CreateValidCourt();
            var newCourtName = CourtName.Of("Updated Name");
            var newSportId = SportId.Of(Guid.NewGuid());
            var newSlotDuration = TimeSpan.FromMinutes(90);
            var newDescription = "Updated Description";
            var newFacilities = "Updated Facilities";
            var newStatus = CourtStatus.Open;
            var newCourtType = CourtType.Outdoor;
            var invalidDepositPercentage = 120m; // Trên 100%

            // Act & Assert
            Assert.Throws<DomainException>(() =>
                court.UpdateCourt(
                    newCourtName,
                    newSportId,
                    newSlotDuration,
                    newDescription,
                    newFacilities,
                    newStatus,
                    newCourtType,
                    invalidDepositPercentage
                ));
        }

        [Fact]
        public void UpdateCancellationPolicy_Should_UpdateProperties_When_Called()
        {
            // Arrange
            var court = CreateValidCourt();
            var newCancellationWindowHours = 48;
            var newRefundPercentage = 75m;

            // Act
            court.UpdateCancellationPolicy(newCancellationWindowHours, newRefundPercentage);

            // Assert
            Assert.Equal(newCancellationWindowHours, court.CancellationWindowHours);
            Assert.Equal(newRefundPercentage, court.RefundPercentage);
        }

        [Fact]
        public void AddCourtSlot_Should_AddNewSlot_When_Called()
        {
            // Arrange
            var court = CreateValidCourt();
            var courtId = court.Id;
            var daysOfWeek = new int[] { 1, 2, 3 }; // Monday, Tuesday, Wednesday
            var startTime = TimeSpan.FromHours(9);
            var endTime = TimeSpan.FromHours(12);
            var priceSlot = 150.0m;

            // Act
            court.AddCourtSlot(courtId, daysOfWeek, startTime, endTime, priceSlot);

            // Assert
            Assert.NotEmpty(court.CourtSchedules);
            var addedSlot = Assert.Single(court.CourtSchedules);
            Assert.Equal(courtId, addedSlot.CourtId);
            Assert.Equal(startTime, addedSlot.StartTime);
            Assert.Equal(endTime, addedSlot.EndTime);
            Assert.Equal(priceSlot, addedSlot.PriceSlot);
        }

        private Court CreateValidCourt()
        {
            return Court.Create(
                CourtId.Of(Guid.NewGuid()),
                CourtName.Of("Tennis Court"),
                SportCenterId.Of(Guid.NewGuid()),
                SportId.Of(Guid.NewGuid()),
                TimeSpan.FromMinutes(60),
                "Description",
                "Facilities",
                CourtType.Indoor,
                100m
            );
        }
    }
}