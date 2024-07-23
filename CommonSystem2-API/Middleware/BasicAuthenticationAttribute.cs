using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;

namespace CommonSystem2_API.Middleware
{
    public class BasicAuthenticationAttribute : Attribute, IAsyncAuthorizationFilter
    {        
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var serviceProvider = context.HttpContext.RequestServices;
            var _configuration = serviceProvider.GetService<IConfiguration>();
            var authHeader = context.HttpContext.Request.Headers["Authorization"].ToString();
            if (authHeader != null && !string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Basic "))
            {
                var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
                var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials)).Split(':');

                if (credentials.Length == 2)
                {
                    var username = credentials[0];
                    var password = credentials[1];                    
                    string expectedUsername = _configuration["BasicAuthSettings:Username"];
                    string expectedPassword = _configuration["BasicAuthSettings:Password"];

                    if (username == expectedUsername && password == expectedPassword)
                        return;
                }
            }
            context.Result = new UnauthorizedResult();
        }
    }
}
