using CourtBooking.Application.CourtManagement.Command.CreateCourt;
using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Enums;
using FluentValidation.TestHelper;
using System;
using System.Collections.Generic;
using Xunit;

namespace CourtBooking.Test.Application.Commands
{
    public class CreateCourtCommandTests
    {
        private readonly CreateCourtCommandValidator _validator;

        public CreateCourtCommandTests()
        {
            _validator = new CreateCourtCommandValidator();
        }

        [Fact]
        public void Constructor_Should_SetProperties_When_Called()
        {
            // Arrange
            var now = DateTime.Now;
            var courtCreateDTO = new CourtCreateDTO(
                CourtName: "Tennis Court 1",
                SportId: Guid.NewGuid(),
                SportCenterId: Guid.NewGuid(),
                Description: "Main court",
                Facilities: null,
                SlotDuration: TimeSpan.FromHours(1),
                MinDepositPercentage: 30,
                CourtType: 1,
                CourtSchedules: new List<CourtScheduleDTO>() {
                    new CourtScheduleDTO(
                        Id: Guid.NewGuid(),
                        CourtId: Guid.NewGuid(),
                        DayOfWeek: new int[] { (int)DayOfWeek.Monday },
                        StartTime: TimeSpan.FromHours(8),
                        EndTime: TimeSpan.FromHours(22),
                        PriceSlot: 100,
                        Status: 1,
                        CreatedAt: now,
                        LastModified: null
                    )
                },
                CancellationWindowHours: 24,
                RefundPercentage: 50
            );

            // Act
            var command = new CreateCourtCommand(courtCreateDTO);

            // Assert
            Assert.Equal(courtCreateDTO, command.Court);
        }

        [Fact]
        public void Validate_Should_Fail_When_CourtCreateDTOIsNull()
        {
            // Arrange
            var command = new CreateCourtCommand(null);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Court);
        }

        [Fact]
        public void Validate_Should_Fail_When_SportCenterIdIsEmpty()
        {
            // Arrange
            var now = DateTime.Now;
            var command = new CreateCourtCommand(
                new CourtCreateDTO(
                    CourtName: "Tennis Court 1",
                    SportId: Guid.NewGuid(),
                    SportCenterId: Guid.Empty, // Empty ID - should fail
                    Description: "Main court",
                    Facilities: null,
                    SlotDuration: TimeSpan.FromHours(1),
                    MinDepositPercentage: 30,
                    CourtType: 1,
                    CourtSchedules: new List<CourtScheduleDTO>() {
                        new CourtScheduleDTO(
                            Id: Guid.NewGuid(),
                            CourtId: Guid.NewGuid(),
                            DayOfWeek: new int[] { (int)DayOfWeek.Monday },
                            StartTime: TimeSpan.FromHours(8),
                            EndTime: TimeSpan.FromHours(22),
                            PriceSlot: 100,
                            Status: 1,
                            CreatedAt: now,
                            LastModified: null
                        )
                    },
                    CancellationWindowHours: 24,
                    RefundPercentage: 50
                )
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Court.SportCenterId);
        }

        [Fact]
        public void Validate_Should_Fail_When_SportIdIsEmpty()
        {
            // Arrange
            var now = DateTime.Now;
            var command = new CreateCourtCommand(
                new CourtCreateDTO(
                    CourtName: "Tennis Court 1",
                    SportId: Guid.Empty, // Empty ID - should fail
                    SportCenterId: Guid.NewGuid(),
                    Description: "Main court",
                    Facilities: null,
                    SlotDuration: TimeSpan.FromHours(1),
                    MinDepositPercentage: 30,
                    CourtType: 1,
                    CourtSchedules: new List<CourtScheduleDTO>() {
                        new CourtScheduleDTO(
                            Id: Guid.NewGuid(),
                            CourtId: Guid.NewGuid(),
                            DayOfWeek: new int[] { (int)DayOfWeek.Monday },
                            StartTime: TimeSpan.FromHours(8),
                            EndTime: TimeSpan.FromHours(22),
                            PriceSlot: 100,
                            Status: 1,
                            CreatedAt: now,
                            LastModified: null
                        )
                    },
                    CancellationWindowHours: 24,
                    RefundPercentage: 50
                )
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Court.SportId);
        }

        [Fact]
        public void Validate_Should_Fail_When_CourtNameIsEmpty()
        {
            // Arrange
            var now = DateTime.Now;
            var command = new CreateCourtCommand(
                new CourtCreateDTO(
                    CourtName: "", // Empty name - should fail
                    SportId: Guid.NewGuid(),
                    SportCenterId: Guid.NewGuid(),
                    Description: "Main court",
                    Facilities: null,
                    SlotDuration: TimeSpan.FromHours(1),
                    MinDepositPercentage: 30,
                    CourtType: 1,
                    CourtSchedules: new List<CourtScheduleDTO>() {
                        new CourtScheduleDTO(
                            Id: Guid.NewGuid(),
                            CourtId: Guid.NewGuid(),
                            DayOfWeek: new int[] { (int)DayOfWeek.Monday },
                            StartTime: TimeSpan.FromHours(8),
                            EndTime: TimeSpan.FromHours(22),
                            PriceSlot: 100,
                            Status: 1,
                            CreatedAt: now,
                            LastModified: null
                        )
                    },
                    CancellationWindowHours: 24,
                    RefundPercentage: 50
                )
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Court.CourtName);
        }

        [Fact]
        public void Validate_Should_Fail_When_CourtNameIsTooLong()
        {
            // Arrange
            var now = DateTime.Now;
            var command = new CreateCourtCommand(
                new CourtCreateDTO(
                    CourtName: new string('A', 101), // 101 characters - should fail
                    SportId: Guid.NewGuid(),
                    SportCenterId: Guid.NewGuid(),
                    Description: "Main court",
                    Facilities: null,
                    SlotDuration: TimeSpan.FromHours(1),
                    MinDepositPercentage: 30,
                    CourtType: 1,
                    CourtSchedules: new List<CourtScheduleDTO>() {
                        new CourtScheduleDTO(
                            Id: Guid.NewGuid(),
                            CourtId: Guid.NewGuid(),
                            DayOfWeek: new int[] { (int)DayOfWeek.Monday },
                            StartTime: TimeSpan.FromHours(8),
                            EndTime: TimeSpan.FromHours(22),
                            PriceSlot: 100,
                            Status: 1,
                            CreatedAt: now,
                            LastModified: null
                        )
                    },
                    CancellationWindowHours: 24,
                    RefundPercentage: 50
                )
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Court.CourtName);
        }

        [Fact]
        public void Validate_Should_Fail_When_MinDepositPercentageIsInvalid()
        {
            // Arrange
            var now = DateTime.Now;
            var command = new CreateCourtCommand(
                new CourtCreateDTO(
                    CourtName: "Tennis Court 1",
                    SportId: Guid.NewGuid(),
                    SportCenterId: Guid.NewGuid(),
                    Description: "Main court",
                    Facilities: null,
                    SlotDuration: TimeSpan.FromHours(1),
                    MinDepositPercentage: 101, // Invalid - should fail
                    CourtType: 1,
                    CourtSchedules: new List<CourtScheduleDTO>() {
                        new CourtScheduleDTO(
                            Id: Guid.NewGuid(),
                            CourtId: Guid.NewGuid(),
                            DayOfWeek: new int[] { (int)DayOfWeek.Monday },
                            StartTime: TimeSpan.FromHours(8),
                            EndTime: TimeSpan.FromHours(22),
                            PriceSlot: 100,
                            Status: 1,
                            CreatedAt: now,
                            LastModified: null
                        )
                    },
                    CancellationWindowHours: 24,
                    RefundPercentage: 50
                )
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Court.MinDepositPercentage);
        }

        [Fact]
        public void Validate_Should_Fail_When_CancellationWindowHoursIsInvalid()
        {
            // Arrange
            var now = DateTime.Now;
            var command = new CreateCourtCommand(
                new CourtCreateDTO(
                    CourtName: "Tennis Court 1",
                    SportId: Guid.NewGuid(),
                    SportCenterId: Guid.NewGuid(),
                    Description: "Main court",
                    Facilities: null,
                    SlotDuration: TimeSpan.FromHours(1),
                    MinDepositPercentage: 30,
                    CourtType: 1,
                    CourtSchedules: new List<CourtScheduleDTO>() {
                        new CourtScheduleDTO(
                            Id: Guid.NewGuid(),
                            CourtId: Guid.NewGuid(),
                            DayOfWeek: new int[] { (int)DayOfWeek.Monday },
                            StartTime: TimeSpan.FromHours(8),
                            EndTime: TimeSpan.FromHours(22),
                            PriceSlot: 100,
                            Status: 1,
                            CreatedAt: now,
                            LastModified: null
                        )
                    },
                    CancellationWindowHours: 0, // Invalid - should fail
                    RefundPercentage: 50
                )
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Court.CancellationWindowHours);
        }

        [Fact]
        public void Validate_Should_Pass_When_AllPropertiesValid()
        {
            // Arrange
            var now = DateTime.Now;
            var command = new CreateCourtCommand(
                new CourtCreateDTO(
                    CourtName: "Tennis Court 1",
                    SportId: Guid.NewGuid(),
                    SportCenterId: Guid.NewGuid(),
                    Description: "Main court",
                    Facilities: null,
                    SlotDuration: TimeSpan.FromHours(1),
                    MinDepositPercentage: 30,
                    CourtType: (int)CourtType.Indoor, // Explicitly cast to ensure valid enum value
                    CourtSchedules: new List<CourtScheduleDTO>() {
                        new CourtScheduleDTO(
                            Id: Guid.NewGuid(),
                            CourtId: Guid.NewGuid(),
                            DayOfWeek: new int[] { (int)DayOfWeek.Monday },
                            StartTime: TimeSpan.FromHours(8),
                            EndTime: TimeSpan.FromHours(22),
                            PriceSlot: 100,
                            Status: 1,
                            CreatedAt: now,
                            LastModified: null
                        )
                    },
                    CancellationWindowHours: 24,
                    RefundPercentage: 50
                )
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}