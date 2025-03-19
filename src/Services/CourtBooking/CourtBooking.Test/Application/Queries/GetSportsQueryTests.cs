using CourtBooking.Application.SportManagement.Queries.GetSports;
using Xunit;

namespace CourtBooking.Test.Application.Queries
{
    public class GetSportsQueryTests
    {
        [Fact]
        public void Constructor_Should_Create_When_Called()
        {
            // Arrange & Act
            var query = new GetSportsQuery();

            // Assert
            Assert.NotNull(query);
        }
    }
}