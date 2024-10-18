using System;

namespace Lucene.Net.ObjectMapping.Tests.Model
{
    public sealed class NestedTestObjectA
    {
        public Guid Id { get; set; }

        public NestedTestObjectB B { get; set; }
    }
}
