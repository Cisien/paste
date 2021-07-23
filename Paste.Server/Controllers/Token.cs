using Paste.Shared;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Paste.Server.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System;

namespace Paste.Server.Controllers
{
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly ITokenService _tokenService;

        public TokenController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost("[action]")]
        [Authorize(Roles = nameof(Permissions.TokensAdmin) +","+ nameof(Permissions.Administrator))]
        public async Task<ActionResult<TokenResponse>> CreateToken(TokenCreateRequest request)
        {
            var callerTokenClaim = User.FindFirst(ClaimTypes.Actor);
            if (callerTokenClaim == null)
            {
                return Unauthorized();
            }

            if (request.Name.Contains(".."))
            {
                return BadRequest("Malformed name");
            }

            try
            {
                var newToken = await _tokenService.CreateTokenAsync(callerTokenClaim.Value, request.Name, request.Permissions);
                return Created(string.Empty, new TokenResponse(newToken.Token, newToken.Name, newToken.Permissions.ToString()));
            }
            catch(InvalidCastException ioe)
            {
                return Problem(ioe.Message);
            }
        }

        [HttpPut("[action]")]
        [Authorize(Roles = nameof(Permissions.TokensAdmin) + "," + nameof(Permissions.Administrator))]
        public async Task<ActionResult<TokenResponse>> UpdateToken(TokenUpdateRequest request)
        {
            var callerTokenClaim = User.FindFirst(ClaimTypes.Actor);
            if (callerTokenClaim == null)
            {
                return Unauthorized();
            }

            if (request.Name.Contains(".."))
            {
                return BadRequest("Malformed name");
            }

            try
            {
                var newToken = await _tokenService.ChangeOthersTokenAsync(callerTokenClaim.Value, request.TokenId, request.Name, request.Permissions);
                return new TokenResponse(newToken.Token, newToken.Name, newToken.Permissions.ToString());
            }
            catch (InvalidOperationException ioe)
            {
                return Problem(ioe.Message);
            }
        }

        [HttpDelete("[action]")]
        [Authorize(Roles = nameof(Permissions.TokensAdmin) + "," + nameof(Permissions.Administrator))]
        public async Task<ActionResult<TokenResponse>> DeleteToken(int id)
        {
            var callerTokenClaim = User.FindFirst(ClaimTypes.Actor);
            if (callerTokenClaim == null)
            {
                return Unauthorized();
            }

            try
            {
                await _tokenService.DeleteOthersTokenAsync(callerTokenClaim.Value, id);
                return NoContent();
            }
            catch(InvalidOperationException ioe)
            {
                return Problem(ioe.Message);
            }
        }

        [HttpPut("[action]")]
        public async Task<ActionResult<TokenResponse>> UpdateOwnToken(OwnTokenUpdateRequest request)
        {
            var callerTokenClaim = User.FindFirst(ClaimTypes.Actor);
            if (callerTokenClaim == null)
            {
                return Unauthorized();
            }

            if (request.Name.Contains(".."))
            {
                return BadRequest("Malformed name");
            }

            try
            {
                var newToken = await _tokenService.ChangeOwnTokenAsync(callerTokenClaim.Value, request.Name);
                return new TokenResponse(newToken.Token, newToken.Name, newToken.Permissions.ToString());
            }
            catch (InvalidOperationException ioe)
            {
                return Problem(ioe.Message);
            }
        }

        [HttpDelete("[action]")]
        public async Task<ActionResult<TokenResponse>> DeleteOwnToken()
        {
            var callerTokenClaim = User.FindFirst(ClaimTypes.Actor);
            if (callerTokenClaim == null)
            {
                return Unauthorized();
            }

            try
            {
                await _tokenService.DeleteOwnTokenAsync(callerTokenClaim.Value);
                return NoContent();
            }
            catch(InvalidOperationException ioe)
            {
                return Problem(ioe.Message);
            }
        }

    }
}
