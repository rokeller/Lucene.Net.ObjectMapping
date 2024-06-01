using System.Collections.Immutable;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Standard;

namespace DirectoryWeb.Analyzers;

internal static class DirectoryAnalyzer
{
    internal static readonly Analyzer Default = CreateDefault();

    private static Analyzer CreateDefault()
    {
        AsciiFoldingAnalyzer asciiFolding = new AsciiFoldingAnalyzer();
        KeywordAnalyzer keyword = new KeywordAnalyzer();

        ImmutableDictionary<string, Analyzer>.Builder builder =
            ImmutableDictionary.CreateBuilder<string, Analyzer>();

        builder.Add("FirstName", asciiFolding);
        builder.Add("LastName", asciiFolding);
        builder.Add("Subjects", keyword);

        return new PerFieldAnalyzerWrapper(
            new StandardAnalyzer(Consts.LuceneVersion),
            builder.ToImmutable());
    }
}
