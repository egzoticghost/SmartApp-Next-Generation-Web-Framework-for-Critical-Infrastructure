namespace SmartWebApi.Services
{
    public interface IJwtService
    {
        string GenerateToken(object user);
    }
}
