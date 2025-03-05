using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Identity.Application.Identity.Queries.UserManagement;
using Identity.Application.Dtos;
using Identity.Domain.Models;
using MediatR;
using Moq;
using Xunit;
using Identity.Application.Data.Repositories;

namespace Identity.Test.Application.Identity.Queries
{
    public class GetUserByIdQueryHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;

        public GetUserByIdQueryHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
        }

        [Fact]
        public async Task Handle_ShouldReturnUserDto_WhenUserExists()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice@example.com",
                IsDeleted = false
            };
            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(user.Id))
                .ReturnsAsync(user);

            var handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object);
            var query = new GetUserByIdQuery(user.Id);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenUserNotFoundOrDeleted()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((User)null);
            var handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object);
            var query = new GetUserByIdQuery(Guid.NewGuid());

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }
    }
}