using Paste.Shared;
using System.Threading.Tasks;

namespace Paste.Server
{
    public interface ITokenService
    {
        Task<ApiToken> ChangeOthersTokenAsync(string ownToken, int otherTokenId, string name, Permissions permissions);
        Task<ApiToken> ChangeOwnTokenAsync(string existingToken, string name);
        Task<ApiToken> CreateTokenAsync(string? ownToken, string name, Permissions permissions);
        Task DeleteOthersTokenAsync(string ownToken, int otherTokenId);
        Task DeleteOwnTokenAsync(string token);
        void EnsureAdminTokenExists();
        Task<ApiToken?> GetTokenAsync(string token);
    }
}