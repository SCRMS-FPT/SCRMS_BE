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
                PhoneNumber = "1234567890",
                BirthDate = DateTime.UtcNow.AddYears(-25),
                Gender = Gender.Female,
                SelfIntroduction = "Test intro",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(user.Id))
                .ReturnsAsync(user);

            // Setup GetRolesAsync to return a non-null collection (empty list if no roles)
            _userRepositoryMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());

            var handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object);
            var query = new GetUserByIdQuery(user.Id);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
            result.FirstName.Should().Be("Alice");
            result.LastName.Should().Be("Smith");
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenUserNotFoundOrDeleted()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((User?)null);
            var handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object);
            var query = new GetUserByIdQuery(Guid.NewGuid());

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ShouldReturnUserProfileDto_WhenUserExistsForProfileQuery()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                FirstName = "Bob",
                LastName = "Johnson",
                Email = "bob@example.com",
                PhoneNumber = "9876543210",
                BirthDate = DateTime.UtcNow.AddYears(-30),
                Gender = Gender.Male,
                SelfIntroduction = "Profile intro",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            var handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object);
            var query = new GetUserProfileByIdQuery(userId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(userId);
            result.FirstName.Should().Be("Bob");
            result.LastName.Should().Be("Johnson");
            result.Email.Should().Be("bob@example.com");
        }
    }
}