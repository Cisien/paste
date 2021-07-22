using System;
using System.Collections.Generic;
using System.Text;

namespace Paste.Shared
{
    public class Upload
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public int Views { get; set; }
        public DateTimeOffset LastViewed { get; set; }
        public virtual ApiToken Owner { get; set; }
    }
}
