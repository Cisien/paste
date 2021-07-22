using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Paste.Server
{
    public class TokenAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ITokenService _tokenSvc;

        public TokenAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, ITokenService tokenSvc) : base(options, logger, encoder, clock)
        {
            _tokenSvc = tokenSvc;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authHeader = Request.Headers.Authorization;
            string? token;
            if (string.IsNullOrWhiteSpace(authHeader))
            {
                if (Request.Query.ContainsKey("authToken"))
                {
                    token = Request.Query["authToken"];
                }
                else
                {
                    Logger.LogWarning("Auth header or authToken url parameter does not exist");
                    return AuthenticateResult.Fail("Auth token missing");
                }
            }
            else
            {
                try
                {
                    var headerValue = AuthenticationHeaderValue.Parse(authHeader);
                    token = headerValue.Parameter;
                }
                catch(Exception ex)
                {
                    Logger.LogWarning(ex, "Auth header exists, but the token couldn't be parsed");
                    return AuthenticateResult.Fail("Invalid token");
                }
            }

            if(string.IsNullOrWhiteSpace(token))
            {
                Logger.LogWarning("Auth header or query string parameter exists, but did not contain a token.");
                return AuthenticateResult.Fail("Invalid Token");
            }
            
            var validToken = await _tokenSvc.GetTokenAsync(token);
            if (validToken == null)
            {
                Logger.LogWarning("The authentication token provided by the user was not found.");
                return AuthenticateResult.Fail("Token not found");
            }

            Logger.LogInformation("Authentication Accepted: {User}", validToken.Name);
            return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name, validToken.Name),
                    new Claim(ClaimTypes.NameIdentifier, validToken.Id.ToString()),
                    new Claim(ClaimTypes.Role, validToken.Permissions.ToString()),
                    new Claim(ClaimTypes.Actor, validToken.Token)
                    }, "token", ClaimTypes.Name, ClaimTypes.Role)), "token"));
        }
    }
}
