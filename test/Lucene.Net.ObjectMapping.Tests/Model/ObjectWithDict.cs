using System;
using System.Collections.Generic;

namespace Lucene.Net.ObjectMapping.Tests.Model
{
    public sealed class ObjectWithDict
    {
        public int Id { get; set; }
        public Dictionary<string, DictValue> StringMap { get; set; }
    }

    public sealed class DictValue
    {
        public string Text { get; set; }
        public int Index { get; set; }
        public DateTime Date { get; set; }
    }
}
