using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Identity.Commands.RefreshToken
{
    public record RefreshTokenCommand(Guid? UserId = null) : ICommand<RefreshTokenResult>;

    public record RefreshTokenResult(
        string Token,
        Guid UserId,
        UserDto User
    );
}