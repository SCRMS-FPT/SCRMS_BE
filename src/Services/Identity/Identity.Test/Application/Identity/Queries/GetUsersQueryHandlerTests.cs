using System;
using System.Collections.Generic;
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
    public class GetUsersQueryHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;

        public GetUsersQueryHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
        }

        [Fact]
        public async Task Handle_ShouldReturnOnlyNonDeletedUsers()
        {
            // Arrange
            var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), FirstName = "Alice", IsDeleted = false },
            new User { Id = Guid.NewGuid(), FirstName = "Bob", IsDeleted = false },
            new User { Id = Guid.NewGuid(), FirstName = "Charlie", IsDeleted = true }
        };
            _userRepositoryMock.Setup(x => x.GetAllUserAsync())
                .ReturnsAsync(users);
            _userRepositoryMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string> { "User" });

            var handler = new GetUsersQueryHandler(_userRepositoryMock.Object);
            var query = new GetUsersQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);
        }
    }
}