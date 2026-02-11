namespace SmartWebApp.Services
{
    public interface IJwtService
    {
        string GenerateToken(object user);
    }
}