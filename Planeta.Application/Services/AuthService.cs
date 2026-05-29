using Planeta.Application.DTOs.Auth;
using Planeta.Application.Interfaces;
using BCrypt.Net;
using Planeta.Domain.Auth;
using Planeta.Domain.Interfaces;

namespace Planeta.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtProvider _jwtProvider;

    public AuthService(IUserRepository userRepository, IJwtProvider jwtProvider)
    {
        _userRepository = userRepository;
        _jwtProvider = jwtProvider;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
       
        var existingEmail = await _userRepository.CheckExistingEmailAsync(request.Email);
        if(existingEmail)
        {
            throw new Exception("Email already exists.");
        }

        bool hasAnyUser = (await _userRepository.GetUsersAsync()).Any();

        string tagetRoleName = hasAnyUser ? "Customer" : "Admin";

        int assignedRoleId = await _userRepository.GetRoleIdByNameAsync(tagetRoleName);

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var newUser = new User
        {
            Email = request.Email,
            UserName = request.UserName,
            HashPassword = hashedPassword,
            PhoneNumber = request.PhoneNumber,
            RoleId = assignedRoleId
        };

        await _userRepository.AddAsync(newUser);

        var userWithRoles = await _userRepository.GetUserByIdWithRolesAsync(newUser.Id);

        if(userWithRoles == null)
        {
            throw new Exception("Error to create user.");
        }

        string token = _jwtProvider.GenerateToken(userWithRoles).ToString();

        return new AuthResponse(token, userWithRoles.UserName, userWithRoles.Role.Name);


    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var userWithEmail = await _userRepository.GetUserByEmailAsync(request.Email);

        if (userWithEmail == null)
        {
            throw new Exception("Invalid email or password");
        }
        
        var user = await _userRepository.GetUserByIdWithRolesAsync(userWithEmail.Id);

        if(user == null)
        {
            throw new Exception("Invalid email or password.");
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.HashPassword);

        if (!isPasswordValid)
        {
            throw new Exception("Invalid email or password");
        }


        string token = _jwtProvider.GenerateToken(user).ToString();


        return new AuthResponse(token, user.UserName, user.Role.Name);
    }
}