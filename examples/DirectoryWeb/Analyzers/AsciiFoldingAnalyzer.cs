using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Standard;

namespace DirectoryWeb.Analyzers;

internal sealed class AsciiFoldingAnalyzer : Analyzer
{
    protected override TokenStreamComponents CreateComponents(
        string fieldName,
        TextReader reader)
    {
        Tokenizer tokenizer = new StandardTokenizer(Consts.LuceneVersion, reader);
        TokenFilter filter = new ASCIIFoldingFilter(
            new LowerCaseFilter(Consts.LuceneVersion, tokenizer));

        return new TokenStreamComponents(tokenizer, filter);
    }
}
