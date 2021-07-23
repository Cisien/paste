using System;

namespace Paste.Server.Models
{
    public record UploadMetaResponse(string Id, string Name, long Size, string ContentType, string Owner, int Views, DateTimeOffset LastViewed, DateTimeOffset Timestamp);
}
