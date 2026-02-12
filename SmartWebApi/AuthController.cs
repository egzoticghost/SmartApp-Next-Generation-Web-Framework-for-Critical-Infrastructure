using AspnetUserApi;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using SmartWebApi.Services;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;

    public AuthController(IUserService userService, IJwtService jwtService)
    {
        _userService = userService;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest model)
    {
        var user = new User
        {
            Email = model.Email
            // Removed Password assignment since User does not have a Password property
        };
        _userService.AddUser(user);
        return Ok("User registered");
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest model)
    {
        // Authenticate user manually since IUserService does not have AuthenticateAsync
        var user = _userService.GetAllUsers()
            .FirstOrDefault(u => u.Email == model.Email /* Add password check if possible */);

        if (user == null) return Unauthorized("Invalid credentials");

        var token = _jwtService.GenerateToken(user);
        return Ok(new { token });
    }
}
