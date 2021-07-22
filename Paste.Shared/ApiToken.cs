using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Paste.Shared
{

    [Index(nameof(Token), IsUnique = true), Index(nameof(Permissions))]
    public class ApiToken
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Token { get; set; }
        public Permissions Permissions { get; set; }
        public virtual ICollection<Upload> Uploads{ get; set; }
    }

    [Flags]
    public enum Permissions
    {
        Unknown = 0x00,

        UploadFiles = 0x1,
        ReplaceFiles = 0x2,
        DeleteFiles = 0x4,
        FilesAdmin = UploadFiles | ReplaceFiles | DeleteFiles,

        UploadNamedFiles = 0x100,
        ReplaceNamedFiles = 0x200,
        DeleteNamedFiles = 0x400,
        NamedFilesAdmin = UploadNamedFiles | ReplaceNamedFiles | DeleteNamedFiles,

        CreateTokens = 0x10000,
        UpdateTokens = 0x20000,
        DeleteTokens = 0x40000,
        TokensAdmin = CreateTokens | UpdateTokens | DeleteTokens,
        
        Administrator = FilesAdmin | NamedFilesAdmin | TokensAdmin
    }
}
