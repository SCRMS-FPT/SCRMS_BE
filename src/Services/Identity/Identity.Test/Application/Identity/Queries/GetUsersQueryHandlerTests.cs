using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Identity.Application.Identity.Queries.UserManagement;
using Microsoft.AspNetCore.Identity;
using FluentAssertions;
using Xunit;
using Identity.Test.Helpers;

namespace Identity.Test.Application.Identity.Queries
{
    public class GetUsersQueryHandlerTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;

        public GetUsersQueryHandlerTests()
        {
            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null);

            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), FirstName = "Alice", IsDeleted = false },
                new User { Id = Guid.NewGuid(), FirstName = "Bob", IsDeleted = false },
                new User { Id = Guid.NewGuid(), FirstName = "Charlie", IsDeleted = true }
            }.AsQueryable();

            // Sử dụng TestAsyncEnumerable từ namespace Identity.Test.Helpers
            var asyncUsers = new TestAsyncEnumerable<User>(users);
            _userManagerMock.Setup(x => x.Users).Returns(asyncUsers);
        }

        [Fact]
        public async Task Handle_ShouldReturnOnlyNonDeletedUsers()
        {
            // Arrange
            var handler = new GetUsersQueryHandler(_userManagerMock.Object);
            var query = new GetUsersQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);
        }
    }
}