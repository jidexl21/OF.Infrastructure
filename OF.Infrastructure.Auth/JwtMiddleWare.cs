using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OF.Infrastructure.Auth.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace OF.Infrastructure.Auth
{

    /// <summary>
    /// 
    /// </summary>
    public class JwtMiddleware : IMiddleware
    {
        //private readonly RequestDelegate _next;
        private readonly IAuthSettings _appSettings;
        private readonly IAuthUserService userService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userService"></param>
        /// <param name="appSettings"></param>
        public JwtMiddleware(IAuthUserService userService, IAuthSettings appSettings)
        {
            //_next = next;
            //_appSettings = appSettings.Value;
            this.userService = userService;
            _appSettings = appSettings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            string token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(new string[] { " " }, StringSplitOptions.None).Last();

            if (token != null)
                await attachUserToContext(context, userService, token);

            await next(context);
        }

        private async Task attachUserToContext(HttpContext context, IAuthUserService userService, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Key);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
                // attach user to context on successful jwt validation
                context.Items["User"] = await userService.GetById(userId);
            }
            catch
            {
                // do nothing if jwt validation fails
                // user is not attached to context so request won't have access to secure routes
            }
        }
    }

}
