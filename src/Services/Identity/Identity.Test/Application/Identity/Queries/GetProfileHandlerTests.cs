using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Application.Identity.Queries.GetProfile;

namespace Identity.Test.Application.Identity.Queries
{
    public class GetProfileHandlerTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;

        public GetProfileHandlerTests()
        {
            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task Handle_ShouldReturnUserProfile_WhenUserExists()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Email = "john@example.com", PhoneNumber = "123456789", BirthDate = DateTime.UtcNow.AddYears(-30), Gender = Gender.Male, CreatedAt = DateTime.UtcNow };
            _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);

            var handler = new GetProfileHandler(_userManagerMock.Object);
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
            _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            var handler = new GetProfileHandler(_userManagerMock.Object);
            var query = new GetProfileQuery(Guid.NewGuid());

            // Act
            Func<Task> act = async () => { await handler.Handle(query, CancellationToken.None); };

            // Assert
            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("User not found");
        }
    }
}