using System;
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

            var handler = new GetProfileHandler(_userRepositoryMock.Object);
            var query = new GetProfileQuery(user.Id);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.FirstName.Should().Be("John");
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenUserNotFound()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((User)null);
            var handler = new GetProfileHandler(_userRepositoryMock.Object);
            var query = new GetProfileQuery(Guid.NewGuid());

            // Act
            Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<DomainException>().WithMessage("User not found");
        }
    }
}