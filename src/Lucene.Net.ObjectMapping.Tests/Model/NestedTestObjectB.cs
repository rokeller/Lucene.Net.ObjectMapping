using System;

namespace Lucene.Net.ObjectMapping.Tests.Model
{
    public sealed class NestedTestObjectB
    {
        public int Id { get; set; }

        public NestedTestObjectC C { get; set; }
    }
}
