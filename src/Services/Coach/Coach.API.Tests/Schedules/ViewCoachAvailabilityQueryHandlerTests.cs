using Coach.API.Data.Models;
using Coach.API.Data.Repositories;
using Coach.API.Features.Schedules.GetCoachSchedules;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Coach.API.Tests.Schedules
{
    public class ViewCoachAvailabilityQueryHandlerTests
    {
        // Normal cases
        [Fact]
        public async Task Handle_ValidRequest_ReturnsSchedules()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var startDate = DateOnly.FromDateTime(DateTime.Now);
            var endDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7));

            // Create weekly schedules for the coach (Monday, Wednesday, Friday)
            var weeklySchedules = new List<CoachSchedule>
            {
                new CoachSchedule { Id = Guid.NewGuid(), CoachId = coachId, DayOfWeek = 1, StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(9)), EndTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(12)) },
                new CoachSchedule { Id = Guid.NewGuid(), CoachId = coachId, DayOfWeek = 3, StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(13)), EndTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(16)) },
                new CoachSchedule { Id = Guid.NewGuid(), CoachId = coachId, DayOfWeek = 5, StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(10)), EndTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(14)) }
            };

            var mockScheduleRepo = new Mock<ICoachScheduleRepository>();
            mockScheduleRepo.Setup(r => r.GetCoachSchedulesByCoachIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(weeklySchedules);

            // Use empty list for bookings
            var bookings = new List<CoachBooking>();
            var mockBookingRepo = new Mock<ICoachBookingRepository>();
            // Create manually what will be returned from GetCoachBookingsByCoachIdAsync
            mockBookingRepo.Setup(r => r.GetCoachBookingsByCoachIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookings);

            var handler = new GetCoachSchedulesQueryHandler(mockScheduleRepo.Object, mockBookingRepo.Object);

            // Act
            var query = new GetCoachSchedulesQuery(coachId, startDate, endDate, 1, 10);
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.RecordPerPage);
            Assert.True(result.TotalRecords > 0);
            Assert.True(result.Schedules.Count > 0);
        }

        [Fact]
        public async Task Handle_NoSchedules_ReturnsEmptyList()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var startDate = DateOnly.FromDateTime(DateTime.Now);
            var endDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7));

            var mockScheduleRepo = new Mock<ICoachScheduleRepository>();
            mockScheduleRepo.Setup(r => r.GetCoachSchedulesByCoachIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CoachSchedule>());

            // Use empty list for bookings
            var bookings = new List<CoachBooking>();
            var mockBookingRepo = new Mock<ICoachBookingRepository>();
            // Create manually what will be returned from GetCoachBookingsByCoachIdAsync
            mockBookingRepo.Setup(r => r.GetCoachBookingsByCoachIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookings);

            var handler = new GetCoachSchedulesQueryHandler(mockScheduleRepo.Object, mockBookingRepo.Object);

            // Act
            var query = new GetCoachSchedulesQuery(coachId, startDate, endDate, 1, 10);
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalRecords);
            Assert.Empty(result.Schedules);
        }

        [Fact]
        public async Task Handle_WithBookings_ReturnsBookedSlots()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var startDate = DateOnly.FromDateTime(DateTime.Now);
            var bookingDate = startDate.AddDays(1); // Tomorrow
            var endDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7));

            var weeklySchedules = new List<CoachSchedule>
            {
                new CoachSchedule { Id = Guid.NewGuid(), CoachId = coachId, DayOfWeek = bookingDate.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)bookingDate.DayOfWeek,
                                  StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(9)),
                                  EndTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(17)) }
            };

            var bookings = new List<CoachBooking>
            {
                new CoachBooking
                {
                    Id = Guid.NewGuid(),
                    CoachId = coachId,
                    BookingDate = bookingDate,
                    StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(10)),
                    EndTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(12)),
                    Status = "pending"
                }
            };

            var mockScheduleRepo = new Mock<ICoachScheduleRepository>();
            mockScheduleRepo.Setup(r => r.GetCoachSchedulesByCoachIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(weeklySchedules);

            var mockBookingRepo = new Mock<ICoachBookingRepository>();
            // Setup to return the bookings list
            mockBookingRepo.Setup(r => r.GetCoachBookingsByCoachIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookings);

            var handler = new GetCoachSchedulesQueryHandler(mockScheduleRepo.Object, mockBookingRepo.Object);

            // Act
            var query = new GetCoachSchedulesQuery(coachId, startDate, endDate, 1, 20);
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.TotalRecords > 0);

            // Check for the booked slot
            var bookedSlots = result.Schedules.Where(s => s.Status == "booked").ToList();
            Assert.NotEmpty(bookedSlots);
        }

        [Fact]
        public async Task Handle_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var startDate = DateOnly.FromDateTime(DateTime.Now);
            var endDate = DateOnly.FromDateTime(DateTime.Now.AddDays(14)); // Two weeks

            // Create daily schedules for two weeks (lots of data for pagination)
            var weeklySchedules = new List<CoachSchedule>();
            for (int i = 1; i <= 7; i++) // All days of the week (1-7)
            {
                weeklySchedules.Add(new CoachSchedule
                {
                    Id = Guid.NewGuid(),
                    CoachId = coachId,
                    DayOfWeek = i,
                    StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(9)),
                    EndTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(17))
                });
            }

            var mockScheduleRepo = new Mock<ICoachScheduleRepository>();
            mockScheduleRepo.Setup(r => r.GetCoachSchedulesByCoachIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(weeklySchedules);

            // Use empty list for bookings
            var bookings = new List<CoachBooking>();
            var mockBookingRepo = new Mock<ICoachBookingRepository>();
            // Create manually what will be returned from GetCoachBookingsByCoachIdAsync
            mockBookingRepo.Setup(r => r.GetCoachBookingsByCoachIdAsync(coachId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookings);

            var handler = new GetCoachSchedulesQueryHandler(mockScheduleRepo.Object, mockBookingRepo.Object);

            // Act - Get the second page with 5 items per page
            var query = new GetCoachSchedulesQuery(coachId, startDate, endDate, 2, 5);
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Page);
            Assert.Equal(5, result.RecordPerPage);
            Assert.True(result.TotalPages > 1);
            Assert.True(result.Schedules.Count <= 5);
        }
    }
}