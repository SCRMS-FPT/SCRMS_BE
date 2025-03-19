using CourtBooking.Application.CourtManagement.Queries.GetSportCenters;
using System;
using BuildingBlocks.Pagination;
using Xunit;

namespace CourtBooking.Test.Application.Queries
{
    public class GetSportCentersQueryTests
    {
        [Fact]
        public void Constructor_Should_Create_When_Called()
        {
            // Arrange & Act
            var query = new GetSportCentersQuery(new PaginationRequest(0, 10));

            // Assert
            Assert.NotNull(query);
        }

        [Fact]
        public void Constructor_WithPaginationRequest_Should_SetPaginationRequest_When_Called()
        {
            // Arrange
            var paginationRequest = new PaginationRequest(0, 10);

            // Act
            var query = new GetSportCentersQuery(
                paginationRequest,
                null,
                null
            );

            // Assert
            Assert.Equal(paginationRequest, query.PaginationRequest);
            Assert.Equal(null, query.City);
            Assert.Equal(null, query.Name);
        }
    }
}