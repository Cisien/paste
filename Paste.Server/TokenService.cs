using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Paste.Shared;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Paste.Server
{
    public class TokenService : ITokenService
    {
        private readonly PasteDbContext _db;
        private readonly ILogger<TokenService> _logger;

        public TokenService(PasteDbContext dbContext, ILogger<TokenService> logger)
        {
            _db = dbContext;
            _logger = logger;
        }

        public void EnsureAdminTokenExists()
        {
            var anyAdminTokens = _db.Tokens.Any(a => a.Permissions == Permissions.Administrator);
            if (!anyAdminTokens)
            {
                var newToken = CreateTokenAsync(null, "Default Administrator", Permissions.Administrator).ConfigureAwait(false).GetAwaiter().GetResult();
                _logger.LogCritical($"=====================Default Administrator Token=====================");
                _logger.LogCritical($"Default administrator token created. Save this token safely, it will only be displayed once! {newToken.Token}");
                _logger.LogCritical($"=====================================================================");
            }
        }

        public async Task<ApiToken?> GetTokenAsync(string token)
        {
            var dbToken = await _db.Tokens.SingleOrDefaultAsync(a => a.Token == token);
            if (dbToken == null)
            {
                return null;
            }

            return dbToken;
        }

        public async Task<ApiToken> CreateTokenAsync(string? ownToken, string name, Permissions permissions)
        {
            if (ownToken != null)
            {
                var ownDbToken = await _db.Tokens.SingleOrDefaultAsync(a => a.Token == ownToken);
                if (ownDbToken == null)
                {
                    throw new InvalidOperationException("Invalid Token");
                }

                if (!ownDbToken.Permissions.HasFlag(Permissions.CreateTokens))
                {
                    throw new InvalidOperationException("Invalid Token");
                }
            }
            else 
            {
                var isFirstAdminToken = permissions == Permissions.Administrator && !await _db.Tokens.AnyAsync(a => a.Permissions == Permissions.Administrator);
                if (!isFirstAdminToken)
                {
                    throw new InvalidOperationException("No token was supplied");
                }
            }

            var token = await GenerateUniqueTokenAsync();

            if(token == null)
            {
                throw new InvalidOperationException("Unable to generate a unique token. Try again.");
            }

            var apiToken = new ApiToken
            {
                Name = name,
                Permissions = permissions,
                Token = token
            };

            _db.Tokens.Add(apiToken);
            await _db.SaveChangesAsync();
            return apiToken;
        }

        public async Task<ApiToken> ChangeOthersTokenAsync(string ownToken, int otherTokenId, string name, Permissions permissions)
        {
            var ownDbToken = await _db.Tokens.SingleOrDefaultAsync(a => a.Token == ownToken);
            if(ownDbToken == null)
            {
                throw new InvalidOperationException("Invalid Token");
            }

            if(!ownDbToken.Permissions.HasFlag(Permissions.UpdateTokens))
            {
                throw new InvalidOperationException("Invalid Token");
            }

            var otherDbToken = await _db.Tokens.FindAsync(otherTokenId);
            if(otherDbToken == null)
            {
                throw new InvalidOperationException($"{otherTokenId} not found");
            }

            if(otherDbToken.Permissions < ownDbToken.Permissions)
            {
                throw new InvalidOperationException("Unable to change token");
            }

            if(permissions < ownDbToken.Permissions)
            {
                throw new InvalidOperationException("Unable to change token");
            }
            
            var newToken = await GenerateUniqueTokenAsync();
            if (newToken == null)
            {
                throw new InvalidOperationException("Unable to generate a unique token. Try again.");
            }

            otherDbToken.Token = newToken;
            await _db.SaveChangesAsync();
            return otherDbToken;
        }

        public async Task<ApiToken> ChangeOwnTokenAsync(string existingToken, string name)
        {
            var dbToken = await _db.Tokens.SingleOrDefaultAsync(a => a.Token == existingToken);
            if(dbToken == null)
            {
                throw new InvalidOperationException("Existing token is invalid.");
            }

            var token = await GenerateUniqueTokenAsync();

            if (token == null)
            {
                throw new InvalidOperationException("Unable to generate a unique token. Try again.");
            }

            dbToken.Token = token;
            dbToken.Name = name;
            await _db.SaveChangesAsync();
            return dbToken;
        }

        public async Task DeleteOthersTokenAsync(string ownToken, int otherTokenId)
        {
            var ownDbToken = await _db.Tokens.SingleOrDefaultAsync(a => a.Token == ownToken);
            if (ownDbToken == null)
            {
                throw new InvalidOperationException("Invalid Token");
            }

            if (!ownDbToken.Permissions.HasFlag(Permissions.UpdateTokens))
            {
                throw new InvalidOperationException("Invalid Token");
            }

            var otherDbToken = await _db.Tokens.FindAsync(otherTokenId);
            if (otherDbToken == null)
            {
                throw new InvalidOperationException($"{otherTokenId} not found");
            }

            if (otherDbToken.Permissions > ownDbToken.Permissions)
            {
                throw new InvalidOperationException("Unable to change token");
            }

            _db.Tokens.Remove(otherDbToken);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteOwnTokenAsync(string token)
        {
            var dbToken = await _db.Tokens.SingleOrDefaultAsync(a => a.Token == token);
            if(dbToken == null)
            {
                return;
            }

            // check to make sure the last admin token isn't being removed
            if(dbToken.Permissions == Permissions.Administrator && await _db.Tokens.CountAsync(a => a.Permissions == Permissions.Administrator) == 1)
            {
                throw new InvalidOperationException("Unable to delete the last admin token");
            }

            _db.Tokens.Remove(dbToken);
            await _db.SaveChangesAsync();
        }

        private async Task<string?> GenerateUniqueTokenAsync(int maxAttempts = 42)
        {
            int attempts = 0;
            string? token = null;
            do
            {
                token = GenerateToken();
                // ensure uniqueness, which is unlikely to collide at small scale, but more likely with a crng guid
                var existingToken = await _db.Tokens.SingleOrDefaultAsync(a => a.Token == token);
                if (existingToken == null)
                {
                    break;
                }
                attempts++;
            } while (attempts < maxAttempts);

            return token;
        }

        private static string GenerateToken()
        {
            var data = new byte[16];
            RandomNumberGenerator.Fill(data);
            return new Guid(data).ToString();
        }

    }
}