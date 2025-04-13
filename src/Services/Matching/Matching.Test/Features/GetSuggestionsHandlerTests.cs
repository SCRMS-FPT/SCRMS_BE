using Matching.API.Data.Repositories;
using Matching.API.Features.Suggestions;
using Matching.API.Data.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Matching.Test.Features
{
    public class GetSuggestionsHandlerTests
    {
        private readonly Mock<IUserSkillRepository> _userSkillRepoMock;
        private readonly GetSuggestionsHandler _handler;

        public GetSuggestionsHandlerTests()
        {
            _userSkillRepoMock = new Mock<IUserSkillRepository>();
            _handler = new GetSuggestionsHandler(_userSkillRepoMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsEmptyList_WhenNoSuggestions()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var filters = new List<SportSkillFilter>();

            _userSkillRepoMock.Setup(m => m.GetSuggestionsWithSkillsAsync(userId, 1, 10, filters, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<Guid, List<UserSkill>>());


            // Act
            var result = await _handler.Handle(new GetSuggestionsQuery(1, 10, userId, filters), CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task Handle_ReturnsUserProfiles_WhenSuggestionsExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var suggestedUser1 = Guid.NewGuid();
            var suggestedUser2 = Guid.NewGuid();
            var filters = new List<SportSkillFilter>();

            var mockData = new Dictionary<Guid, List<UserSkill>>
            {
                {
                    suggestedUser1,
                    new List<UserSkill>
                    {
                        new UserSkill { SportId = Guid.NewGuid(), SkillLevel = "Intermediate" }
                    }
                },
                {
                    suggestedUser2,
                    new List<UserSkill>
                    {
                        new UserSkill { SportId = Guid.NewGuid(), SkillLevel = "Advanced" }
                    }
                }
            };

            _userSkillRepoMock.Setup(m => m.GetSuggestionsWithSkillsAsync(userId, 1, 10, filters, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockData);

            // Act
            var result = await _handler.Handle(new GetSuggestionsQuery(1, 10, userId, filters), CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.Id == suggestedUser1);
            Assert.Contains(result, r => r.Id == suggestedUser2);
        }

        [Fact]
        public async Task Handle_RespectsPagination()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var filters = new List<SportSkillFilter>();

            var allSuggestions = new Dictionary<Guid, List<UserSkill>>
            {
                {
                    Guid.NewGuid(),
                    new List<UserSkill> { new UserSkill { SportId = Guid.NewGuid(), SkillLevel = "Beginner" } }
                },
                {
                    Guid.NewGuid(),
                    new List<UserSkill> { new UserSkill { SportId = Guid.NewGuid(), SkillLevel = "Advanced" } }
                },
                {
                    Guid.NewGuid(),
                    new List<UserSkill> { new UserSkill { SportId = Guid.NewGuid(), SkillLevel = "Intermediate" } }
                }
            };

            var pagedSuggestions = allSuggestions.Skip(1).Take(1)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            _userSkillRepoMock.Setup(m => m.GetSuggestionsWithSkillsAsync(userId, 2, 1, filters, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedSuggestions);

            // Act
            var result = await _handler.Handle(new GetSuggestionsQuery(2, 1, userId, filters), CancellationToken.None);

            // Assert
            Assert.Single(result);
            Assert.Equal(pagedSuggestions.First().Key, result[0].Id);
        }

        [Fact]
        public async Task Handle_WithSportSkillFilters_AppliesFiltersCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var sportId1 = Guid.NewGuid();
            var sportId2 = Guid.NewGuid();
            var filters = new List<SportSkillFilter>
            {
                new SportSkillFilter { SportId = sportId1, SkillLevel = "Intermediate" },
                new SportSkillFilter { SportId = sportId2, SkillLevel = "Advanced" }
            };

            var suggestedUser = Guid.NewGuid();
            var mockData = new Dictionary<Guid, List<UserSkill>>
            {
                {
                    suggestedUser,
                    new List<UserSkill>
                    {
                        new UserSkill { SportId = sportId1, SkillLevel = "Intermediate" },
                        new UserSkill { SportId = sportId2, SkillLevel = "Advanced" }
                    }
                }
            };

            _userSkillRepoMock.Setup(m => m.GetSuggestionsWithSkillsAsync(userId, 1, 10, filters, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockData);

            // Act
            var result = await _handler.Handle(new GetSuggestionsQuery(1, 10, userId, filters), CancellationToken.None);

            // Assert
            Assert.Single(result);
            Assert.Equal(suggestedUser, result[0].Id);
            _userSkillRepoMock.Verify(m => m.GetSuggestionsWithSkillsAsync(userId, 1, 10,
                It.Is<List<SportSkillFilter>>(f =>
                    f.Count == 2 &&
                    f[0].SportId == sportId1 &&
                    f[0].SkillLevel == "Intermediate" &&
                    f[1].SportId == sportId2 &&
                    f[1].SkillLevel == "Advanced"),
                It.IsAny<CancellationToken>()),
                Times.Once());
        }

        [Fact]
        public async Task Handle_MapsSkillsCorrectly()
        {
            // Arrange - Testing that skills are correctly mapped to user profiles
            var userId = Guid.NewGuid();
            var suggestedUser = Guid.NewGuid();
            var sportId1 = Guid.NewGuid();
            var sportId2 = Guid.NewGuid();
            var filters = new List<SportSkillFilter>();

            var mockData = new Dictionary<Guid, List<UserSkill>>
            {
                {
                    suggestedUser,
                    new List<UserSkill>
                    {
                        new UserSkill { SportId = sportId1, SkillLevel = "Beginner" },
                        new UserSkill { SportId = sportId2, SkillLevel = "Advanced" }
                    }
                }
            };

            _userSkillRepoMock.Setup(m => m.GetSuggestionsWithSkillsAsync(userId, 1, 10, filters, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockData);

            // Act
            var result = await _handler.Handle(new GetSuggestionsQuery(1, 10, userId, filters), CancellationToken.None);

            // Assert
            Assert.Single(result);
            var profile = result[0];
            Assert.Equal(suggestedUser, profile.Id);
            Assert.Equal(2, profile.Sports.Count);

            Assert.Contains(profile.Sports, s => s.SportId == sportId1 && s.SkillLevel == "Beginner");
            Assert.Contains(profile.Sports, s => s.SportId == sportId2 && s.SkillLevel == "Advanced");
        }
    }
}

