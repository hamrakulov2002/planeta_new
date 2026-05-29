using Planeta.Domain.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Planeta.Domain.Interfaces;

public interface IJwtProvider
{
    string GenerateToken(User user);
}
