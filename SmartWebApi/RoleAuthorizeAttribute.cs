using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _role;

    public RoleAuthorizeAttribute(string role)
    {
        _role = role;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (!user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

        if (userRole != _role)
        {
            context.Result = new ForbidResult();
        }
    }
}
