using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Application.Identity.Commands.UpdateProfile;

namespace Identity.Test.Application.Identity.Commands
{
    public class UpdateProfileHandlerTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;

        public UpdateProfileHandlerTests()
        {
            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task Handle_ShouldUpdateProfileSuccessfully()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), FirstName = "Old", LastName = "Name", PhoneNumber = "123", BirthDate = DateTime.UtcNow.AddYears(-25), Gender = Gender.Male, Email = "test@example.com", CreatedAt = DateTime.UtcNow };
            _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            var handler = new UpdateProfileHandler(_userManagerMock.Object);
            var command = new UpdateProfileCommand(user.Id, "NewFirst", "NewLast", "+987654321", DateTime.UtcNow.AddYears(-20), "Male");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.FirstName.Should().Be("NewFirst");
            result.LastName.Should().Be("NewLast");
            result.Phone.Should().Be("+987654321");
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenUserNotFound()
        {
            // Arrange
            _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            var handler = new UpdateProfileHandler(_userManagerMock.Object);
            var command = new UpdateProfileCommand(Guid.NewGuid(), "NewFirst", "NewLast", "+987654321", DateTime.UtcNow.AddYears(-20), "Male");

            // Act
            Func<Task> act = async () => { await handler.Handle(command, CancellationToken.None); };

            // Assert
            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("User not found");
        }
    }
}