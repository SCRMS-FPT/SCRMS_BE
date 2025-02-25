using Identity.Test.Helpers;
using Identity.Application.Identity.Queries.UserManagement;
using Identity.Domain.Models;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Identity.Test.Application.Identity.Queries
{
    public class GetUserByIdQueryHandlerTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;

        public GetUserByIdQueryHandlerTests()
        {
            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null);

            // Tạo một danh sách user mẫu
            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), FirstName = "Alice", LastName = "Smith", Email = "alice@example.com", IsDeleted = false },
                new User { Id = Guid.NewGuid(), FirstName = "Bob", LastName = "Jones", Email = "bob@example.com", IsDeleted = false }
            }.AsQueryable();

            // Sử dụng TestAsyncEnumerable để chuyển IQueryable thành async
            var asyncUsers = new TestAsyncEnumerable<User>(users);

            _userManagerMock.Setup(x => x.Users).Returns(asyncUsers);
        }

        [Fact]
        public async Task Handle_ShouldReturnUserDto_WhenUserExists()
        {
            // Arrange
            var existingUser = _userManagerMock.Object.Users.First();
            var handler = new GetUserByIdQueryHandler(_userManagerMock.Object);
            var query = new GetUserByIdQuery(existingUser.Id);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(existingUser.Id);
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenUserNotFoundOrDeleted()
        {
            // Arrange
            var handler = new GetUserByIdQueryHandler(_userManagerMock.Object);
            var query = new GetUserByIdQuery(Guid.NewGuid());

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }
    }
}