using Matching.API.Data;
using Matching.API.Data.Models;
using Matching.API.Data.Repositories;
using Matching.API.Features.Skills.CreateUserSkill;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentValidation.TestHelper;

namespace Matching.Test.Features
{
    public class CreateUserSkillHandlerTests
    {
        private readonly Mock<IUserSkillRepository> _userSkillRepoMock;
        private readonly Mock<MatchingDbContext> _contextMock;
        private readonly CreateUserSkillHandler _handler;
        private readonly CreateUserSkillValidator _validator;

        public CreateUserSkillHandlerTests()
        {
            _userSkillRepoMock = new Mock<IUserSkillRepository>();
            var options = new DbContextOptionsBuilder<MatchingDbContext>()
                .UseInMemoryDatabase("TestDB")
                .Options;
            _contextMock = new Mock<MatchingDbContext>(options);
            _handler = new CreateUserSkillHandler(_userSkillRepoMock.Object, _contextMock.Object);
            _validator = new CreateUserSkillValidator();
        }

        [Fact]
        public async Task Handle_NewSkill_CreatesNewSkill()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var sportId = Guid.NewGuid();
            var command = new CreateUserSkillCommand(userId, sportId, "Intermediate");
            
            _userSkillRepoMock.Setup(m => m.GetByUserIdAndSportIdAsync(userId, sportId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserSkill?)null); // Fixed: Added ? to make it nullable
            _contextMock.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _userSkillRepoMock.Verify(m => m.AddUserSkillAsync(
                It.Is<UserSkill>(s => 
                    s.UserId == userId && 
                    s.SportId == sportId && 
                    s.SkillLevel == "Intermediate"),
                It.IsAny<CancellationToken>()), 
                Times.Once());
            _contextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task Handle_ExistingSkill_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var sportId = Guid.NewGuid();
            var command = new CreateUserSkillCommand(userId, sportId, "Beginner");
            
            var existingSkill = new UserSkill
            {
                UserId = userId,
                SportId = sportId,
                SkillLevel = "Advanced"
            };
            
            _userSkillRepoMock.Setup(m => m.GetByUserIdAndSportIdAsync(userId, sportId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingSkill);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
            
            _userSkillRepoMock.Verify(m => m.AddUserSkillAsync(It.IsAny<UserSkill>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public void Validate_EmptyUserId_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateUserSkillCommand(Guid.Empty, Guid.NewGuid(), "Beginner");

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UserId);
        }

        [Fact]
        public void Validate_EmptySportId_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateUserSkillCommand(Guid.NewGuid(), Guid.Empty, "Beginner");

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SportId);
        }

        [Fact]
        public void Validate_EmptySkillLevel_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateUserSkillCommand(Guid.NewGuid(), Guid.NewGuid(), string.Empty);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SkillLevel);
        }

        [Theory]
        [InlineData("Beginner", true)]
        [InlineData("Intermediate", true)]
        [InlineData("Advanced", true)]
        [InlineData("Expert", false)]
        [InlineData("Novice", false)]
        public void Validate_SkillLevel_ShouldValidateCorrectly(string skillLevel, bool shouldBeValid)
        {
            // Arrange
            var command = new CreateUserSkillCommand(Guid.NewGuid(), Guid.NewGuid(), skillLevel);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            if (shouldBeValid)
            {
                result.ShouldNotHaveValidationErrorFor(x => x.SkillLevel);
            }
            else
            {
                result.ShouldHaveValidationErrorFor(x => x.SkillLevel);
            }
        }
    }
}