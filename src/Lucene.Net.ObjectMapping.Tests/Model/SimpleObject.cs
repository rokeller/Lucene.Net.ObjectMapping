using System;

namespace Lucene.Net.ObjectMapping.Tests.Model
{
    public sealed class SimpleObject
    {
        public Guid Id { get; set; }

        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
