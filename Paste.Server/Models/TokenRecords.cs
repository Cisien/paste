using Paste.Shared;

namespace Paste.Server.Models
{
    public record TokenCreateRequest(string Name, Permissions Permissions);
    public record TokenUpdateRequest(string Name, int TokenId, Permissions Permissions);
    public record OwnTokenUpdateRequest(string Name);
    public record TokenResponse(string Token, string Name, string Permissions);
}
