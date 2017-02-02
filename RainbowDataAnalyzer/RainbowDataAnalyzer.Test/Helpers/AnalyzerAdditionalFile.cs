namespace RainbowDataAnalyzer.Test.Helpers
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;

    public sealed class AnalyzerAdditionalFile : AdditionalText
    {
        private readonly string path;
        private readonly string yamlContents;

        public AnalyzerAdditionalFile(string path, string yamlContents)
        {
            this.path = path;
            this.yamlContents = yamlContents;
        }

        public override string Path => path;

        public override SourceText GetText(CancellationToken cancellationToken)
        {
            return SourceText.From(this.yamlContents);
        }
    }
}
