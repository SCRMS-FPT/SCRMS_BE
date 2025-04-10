using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Identity.Application.Dtos;
using Identity.Domain.Exceptions;
using Identity.Domain.Models;
using MediatR;
using Moq;
using Xunit;
using Identity.Application.Data.Repositories;
using Identity.Application.Identity.Queries.GetProfile;

namespace Identity.Test.Application.Identity.Queries
{
    public class GetProfileHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;

        public GetProfileHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
        }

        [Fact]
        public async Task Handle_ShouldReturnUserProfile_WhenUserExists()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                PhoneNumber = "123456789",
                BirthDate = DateTime.UtcNow.AddYears(-30),
                Gender = Gender.Male,
                CreatedAt = DateTime.UtcNow
            };
            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(user.Id))
                .ReturnsAsync(user);
            
            // Setup GetRolesAsync to return a non-null collection (empty list if no roles)
            _userRepositoryMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());

            var handler = new GetProfileHandler(_userRepositoryMock.Object);
            var query = new GetProfileQuery(user.Id);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Doe");
            result.Email.Should().Be("john@example.com");
            result.Roles.Should().NotBeNull();
            result.Roles.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenUserNotFound()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((User?)null);
            var handler = new GetProfileHandler(_userRepositoryMock.Object);
            var query = new GetProfileQuery(Guid.NewGuid());

            // Act
            Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<DomainException>().WithMessage("User not found");
        }
        
        [Fact]
        public async Task Handle_ShouldIncludeRoles_WhenUserHasRoles()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com",
                PhoneNumber = "987654321",
                BirthDate = DateTime.UtcNow.AddYears(-25),
                Gender = Gender.Female,
                CreatedAt = DateTime.UtcNow
            };
            
            var roles = new List<string> { "Admin", "User" };
            
            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(user.Id))
                .ReturnsAsync(user);
                
            _userRepositoryMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(roles);

            var handler = new GetProfileHandler(_userRepositoryMock.Object);
            var query = new GetProfileQuery(user.Id);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Roles.Should().NotBeNull();
            result.Roles.Should().BeEquivalentTo(roles);
            result.Roles.Should().Contain("Admin");
            result.Roles.Should().Contain("User");
        }
    }
}