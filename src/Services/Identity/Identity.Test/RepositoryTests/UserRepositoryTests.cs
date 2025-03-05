// File: UserRepositoryTests.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Identity.Application.Data.Repositories;
using Identity.Domain.Models;
using Identity.Infrastructure.Data.Repositories;
using Identity.Test.Helpers; // Import the async helper classes
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace Identity.Test.RepositoryTests
{
    public class UserRepositoryTests
    {
        // Helper method to create a mocked UserManager<User>
        private Mock<UserManager<User>> GetUserManagerMock()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsUser_WhenExists()
        {
            // Arrange
            var userManagerMock = GetUserManagerMock();
            var user = new User { Id = Guid.NewGuid(), Email = "test@example.com" };
            userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString()))
                           .ReturnsAsync(user);
            var repo = new UserRepository(userManagerMock.Object);

            // Act
            var result = await repo.GetUserByIdAsync(user.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var userManagerMock = GetUserManagerMock();
            userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                           .ReturnsAsync((User)null);
            var repo = new UserRepository(userManagerMock.Object);

            // Act
            var result = await repo.GetUserByIdAsync(Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsUser_WhenExists()
        {
            // Arrange
            var userManagerMock = GetUserManagerMock();
            var user = new User { Id = Guid.NewGuid(), Email = "existing@example.com" };
            userManagerMock.Setup(m => m.FindByEmailAsync("existing@example.com"))
                           .ReturnsAsync(user);
            var repo = new UserRepository(userManagerMock.Object);

            // Act
            var result = await repo.GetUserByEmailAsync("existing@example.com");

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be("existing@example.com");
        }

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var userManagerMock = GetUserManagerMock();
            userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                           .ReturnsAsync((User)null);
            var repo = new UserRepository(userManagerMock.Object);

            // Act
            var result = await repo.GetUserByEmailAsync("nonexistent@example.com");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateUserAsync_ReturnsSuccess_WhenUserIsValid()
        {
            // Arrange
            var userManagerMock = GetUserManagerMock();
            var user = new User { Email = "test@example.com" };
            userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>(), "Password123"))
                           .ReturnsAsync(IdentityResult.Success)
                           .Callback<User, string>((u, p) => { u.Id = Guid.NewGuid(); });
            var repo = new UserRepository(userManagerMock.Object);

            // Act
            var result = await repo.CreateUserAsync(new User { Email = "test@example.com" }, "Password123");

            // Assert
            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task CreateUserAsync_ReturnsFailed_WhenCreationFails()
        {
            // Arrange
            var userManagerMock = GetUserManagerMock();
            userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>(), "Password123"))
                           .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Duplicate email" }));
            var repo = new UserRepository(userManagerMock.Object);

            // Act
            var result = await repo.CreateUserAsync(new User { Email = "duplicate@example.com" }, "Password123");

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Errors.First().Description.Should().Be("Duplicate email");
        }

        [Fact]
        public async Task UpdateUserAsync_ReturnsSuccess_WhenUserUpdated()
        {
            // Arrange
            var userManagerMock = GetUserManagerMock();
            var user = new User { Id = Guid.NewGuid(), FirstName = "Old" };
            userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
            var repo = new UserRepository(userManagerMock.Object);
            user.FirstName = "New";

            // Act
            var result = await repo.UpdateUserAsync(user);

            // Assert
            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteUserAsync_SetsIsDeletedAndReturnsSuccess()
        {
            // Arrange
            var userManagerMock = GetUserManagerMock();
            var user = new User { Id = Guid.NewGuid(), IsDeleted = false };
            userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
            var repo = new UserRepository(userManagerMock.Object);

            // Act
            var result = await repo.DeleteUserAsync(user);

            // Assert
            result.Succeeded.Should().BeTrue();
            user.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public async Task GetRolesAsync_ReturnsRoles_WhenUserExists()
        {
            // Arrange
            var userManagerMock = GetUserManagerMock();
            var user = new User { Id = Guid.NewGuid() };
            var roles = new List<string> { "Admin", "User" };
            userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(roles);
            var repo = new UserRepository(userManagerMock.Object);

            // Act
            var result = await repo.GetRolesAsync(user);

            // Assert
            result.Should().BeEquivalentTo(roles);
        }

        [Fact]
        public async Task AddToRoleAsync_ReturnsSuccess_WhenRoleAdded()
        {
            // Arrange
            var userManagerMock = GetUserManagerMock();
            var user = new User { Id = Guid.NewGuid() };
            userManagerMock.Setup(m => m.AddToRoleAsync(user, "Admin")).ReturnsAsync(IdentityResult.Success);
            var repo = new UserRepository(userManagerMock.Object);

            // Act
            var result = await repo.AddToRoleAsync(user, "Admin");

            // Assert
            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task RemoveFromRolesAsync_ReturnsSuccess_WhenRolesRemoved()
        {
            // Arrange
            var userManagerMock = GetUserManagerMock();
            var user = new User { Id = Guid.NewGuid() };
            var roles = new List<string> { "User", "Admin" };
            userManagerMock.Setup(m => m.RemoveFromRolesAsync(user, roles)).ReturnsAsync(IdentityResult.Success);
            var repo = new UserRepository(userManagerMock.Object);

            // Act
            var result = await repo.RemoveFromRolesAsync(user, roles);

            // Assert
            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task AddToRolesAsync_ReturnsSuccess_WhenRolesAdded()
        {
            // Arrange
            var userManagerMock = GetUserManagerMock();
            var user = new User { Id = Guid.NewGuid() };
            var roles = new List<string> { "Admin", "User" };
            userManagerMock.Setup(m => m.AddToRolesAsync(user, roles)).ReturnsAsync(IdentityResult.Success);
            var repo = new UserRepository(userManagerMock.Object);

            // Act
            var result = await repo.AddToRolesAsync(user, roles);

            // Assert
            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task CheckPasswordAsync_ReturnsTrue_WhenPasswordIsCorrect()
        {
            // Arrange
            var userManagerMock = GetUserManagerMock();
            var user = new User { Id = Guid.NewGuid() };
            userManagerMock.Setup(m => m.CheckPasswordAsync(user, "CorrectPass")).ReturnsAsync(true);
            var repo = new UserRepository(userManagerMock.Object);

            // Act
            var result = await repo.CheckPasswordAsync(user, "CorrectPass");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CheckPasswordAsync_ReturnsFalse_WhenPasswordIsIncorrect()
        {
            // Arrange
            var userManagerMock = GetUserManagerMock();
            var user = new User { Id = Guid.NewGuid() };
            userManagerMock.Setup(m => m.CheckPasswordAsync(user, "WrongPass")).ReturnsAsync(false);
            var repo = new UserRepository(userManagerMock.Object);

            // Act
            var result = await repo.CheckPasswordAsync(user, "WrongPass");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdatePasswordAsync_ReturnsSuccess_WhenOldPasswordIsCorrect()
        {
            // Arrange
            var userManagerMock = GetUserManagerMock();
            var user = new User { Id = Guid.NewGuid() };
            userManagerMock.Setup(m => m.CheckPasswordAsync(user, "OldPass")).ReturnsAsync(true);
            string token = "dummyToken";
            userManagerMock.Setup(m => m.GeneratePasswordResetTokenAsync(user)).ReturnsAsync(token);
            userManagerMock.Setup(m => m.ResetPasswordAsync(user, token, "NewPass")).ReturnsAsync(IdentityResult.Success);
            var repo = new UserRepository(userManagerMock.Object);

            // Act
            var result = await repo.UpdatePasswordAsync(user, "OldPass", "NewPass");

            // Assert
            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task UpdatePasswordAsync_ReturnsFailed_WhenOldPasswordIsIncorrect()
        {
            // Arrange
            var userManagerMock = GetUserManagerMock();
            var user = new User { Id = Guid.NewGuid() };
            userManagerMock.Setup(m => m.CheckPasswordAsync(user, "WrongOld")).ReturnsAsync(false);
            var repo = new UserRepository(userManagerMock.Object);

            // Act
            var result = await repo.UpdatePasswordAsync(user, "WrongOld", "NewPass");

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Errors.First().Description.Should().Be("Mật khẩu cũ không đúng");
        }

        [Fact]
        public async Task GetAllUserAsync_ReturnsUsers_WhenUsersExist()
        {
            // Arrange
            var userManagerMock = GetUserManagerMock();
            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), Email = "a@example.com" },
                new User { Id = Guid.NewGuid(), Email = "b@example.com" }
            };
            // Use the TestAsyncEnumerable helper to enable async operations.
            userManagerMock.Setup(m => m.Users).Returns(new TestAsyncEnumerable<User>(users));
            var repo = new UserRepository(userManagerMock.Object);

            // Act
            var result = await repo.GetAllUserAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllUserAsync_ReturnsEmptyList_WhenNoUsersExist()
        {
            // Arrange
            var userManagerMock = GetUserManagerMock();
            userManagerMock.Setup(m => m.Users).Returns(new TestAsyncEnumerable<User>(new List<User>()));
            var repo = new UserRepository(userManagerMock.Object);

            // Act
            var result = await repo.GetAllUserAsync();

            // Assert
            result.Should().BeEmpty();
        }
    }
}