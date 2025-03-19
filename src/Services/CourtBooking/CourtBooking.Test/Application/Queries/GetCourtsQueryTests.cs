using BuildingBlocks.Pagination;
using CourtBooking.Application.CourtManagement.Queries.GetCourts;
using Xunit;
using System;

namespace CourtBooking.Test.Application.Queries
{
    public class GetCourtsQueryTests
    {
        [Fact]
        public void Constructor_Should_Create_When_Called()
        {
            // Arrange
            var paginationRequest = new PaginationRequest(1, 10);
            Guid? sportCenterId = null;
            Guid? sportId = null;
            string? courtType = null;

            // Act
            var query = new GetCourtsQuery(paginationRequest, sportCenterId, sportId, courtType);

            // Assert
            Assert.NotNull(query);
        }
    }
}