namespace RainbowDataAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// This Roslyn analyzer checks if Sitecore ID's and paths are valid in serialized Rainbow data.
    /// </summary>
    /// <seealso cref="Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer" />
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RainbowDataAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        private static DiagnosticDescriptor RuleForIds = new DiagnosticDescriptor(
            "RainbowDataAnalyzerIds",
            new LocalizableResourceString(nameof(Resources.IdsAnalyzerTitle), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.IdsAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources)),
            "Sitecore",
            DiagnosticSeverity.Error,
            true,
            new LocalizableResourceString(nameof(Resources.IdsAnalyzerDescription), Resources.ResourceManager, typeof(Resources)));

        private static DiagnosticDescriptor RuleForPaths = new DiagnosticDescriptor(
            "RainbowDataAnalyzerPaths",
            new LocalizableResourceString(nameof(Resources.PathsAnalyzerTitle), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.PathsAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources)),
            "Sitecore",
            DiagnosticSeverity.Error,
            true,
            new LocalizableResourceString(nameof(Resources.PathsAnalyzerDescription), Resources.ResourceManager, typeof(Resources)));
        
        private static DiagnosticDescriptor RuleForFieldIds = new DiagnosticDescriptor(
            "RainbowDataAnalyzerFieldIds",
            new LocalizableResourceString(nameof(Resources.FieldIdsAnalyzerTitle), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.FieldIdsAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources)),
            "Sitecore",
            DiagnosticSeverity.Error,
            true,
            new LocalizableResourceString(nameof(Resources.FieldIdsAnalyzerDescription), Resources.ResourceManager, typeof(Resources)));
        
        private static DiagnosticDescriptor RuleForFieldNames = new DiagnosticDescriptor(
            "RainbowDataAnalyzerFieldPaths",
            new LocalizableResourceString(nameof(Resources.FieldPathsAnalyzerTitle), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.FieldPathsAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources)),
            "Sitecore",
            DiagnosticSeverity.Error,
            true,
            new LocalizableResourceString(nameof(Resources.FieldPathsAnalyzerDescription), Resources.ResourceManager, typeof(Resources)));

        /// <summary>
        /// The names of possible Sitecore item types.
        /// </summary>
        private static readonly string[] sitecoreItemFullNames =
            {
                "Sitecore.Data.Items.BaseItem",
                "Sitecore.Data.Items.Item"
            };

        /// <summary>
        /// The ID for the template of a template field
        /// </summary>
        private static readonly Guid sitecoreTemplateFieldId = Guid.Parse("{455A3E98-A627-4B40-8035-E683A0331AC7}");

        /// <summary>
        /// Rainbow file hashes that can be used to keep reference to caches for already parsed files.
        /// </summary>
        private readonly Dictionary<string, int> rainbowFileHashes = new Dictionary<string, int>();

        /// <summary>
        /// Cached versions of parsed .yml files.
        /// </summary>
        private readonly Dictionary<string, RainbowFile> rainbowFiles = new Dictionary<string, RainbowFile>();

        /// <summary>
        /// Returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(RuleForIds, RuleForPaths, RuleForFieldIds, RuleForFieldNames); } }

        /// <summary>
        /// Called once at session start to register actions in the analysis context.
        /// </summary>
        /// <param name="context"></param>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(this.AnalyzeSyntaxNodeAction, new[] { SyntaxKind.StringLiteralExpression });
        }

        /// <summary>
        /// Analyzes the syntax node action.
        /// </summary>
        /// <param name="context">The context.</param>
        private void AnalyzeSyntaxNodeAction(SyntaxNodeAnalysisContext context)
        {
            // Keep start time so we can determine how long this action will take
            DateTime startTime = DateTime.Now;

            try
            {
                // If there are no .yml files available in the project, we should not validate
                if (!context.Options.AdditionalFiles.Any())
                {
                    return;
                }

                // If this ID/path refers to a field, we should apply more strict validation
                bool validateAsField = ShouldValidateAsField(context);

                // Check all string literal expressions
                var pathOrId = context.Node.ToString().TrimStart('@').Trim('"');
                Guid valueAsId;
                if (Guid.TryParse(pathOrId, out valueAsId))
                {
                    // Exclude guids that are obviously not Sitecore ID's
                    if(context.Node.Ancestors().Any(n =>
                            n is AttributeSyntax
                            && nameof(Guid).Equals(((AttributeSyntax) n).Name.ToString(), StringComparison.OrdinalIgnoreCase)))
                    {
                        return;
                    }

                    // All guids are regarded as Sitecore ID's and will be checked
                    RainbowFile matchingFile;
                    if (!this.Evaluate(context.Options.AdditionalFiles, file => Guid.Equals(file.Id, valueAsId), out matchingFile))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(RuleForIds, context.Node.GetLocation(), pathOrId));
                        return;
                    }

                    if (validateAsField && matchingFile.TemplateId != sitecoreTemplateFieldId)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(RuleForFieldIds, context.Node.GetLocation(), pathOrId));
                    }
                }
                else if (pathOrId.StartsWith("/sitecore/", StringComparison.OrdinalIgnoreCase))
                {
                    // All paths that start with /sitecore/ are regarded as Sitecore paths and will be checked
                    RainbowFile matchingFile;
                    if (!this.Evaluate(context.Options.AdditionalFiles, file => string.Equals(file.Path, pathOrId.TrimEnd('/'), StringComparison.OrdinalIgnoreCase), out matchingFile))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(RuleForPaths, context.Node.GetLocation(), pathOrId));
                    }
                }
                else if (validateAsField && !pathOrId.Contains("/"))
                {
                    RainbowFile matchingFile;
                    if (!this.Evaluate(context.Options.AdditionalFiles, file => file.TemplateId == sitecoreTemplateFieldId
                        && string.Equals(file.ItemName, pathOrId, StringComparison.OrdinalIgnoreCase), out matchingFile))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(RuleForFieldNames, context.Node.GetLocation(), pathOrId));
                    }
                }
            }
            finally
            {
                Debug.WriteLine($"Analyzed syntax node action in {(DateTime.Now - startTime).Milliseconds}ms");
            }
        }

        private static bool ShouldValidateAsField(SyntaxNodeAnalysisContext context)
        {
            SyntaxNode bracketsNode = context.Node.Ancestors()
                .FirstOrDefault(a => a is BracketedArgumentListSyntax || a is ArgumentListSyntax);
            SyntaxNode bracketsOperateOnNode = bracketsNode?.Parent.ChildNodes().First().ChildNodes()
                .LastOrDefault(c => c is IdentifierNameSyntax) as IdentifierNameSyntax;

            if (bracketsOperateOnNode != null && new [] { "Fields", "Field", "BeginField" }.Contains(bracketsOperateOnNode.ToString()))
            {
                return true;
            }

            if (bracketsNode?.Parent != null &&
                     sitecoreItemFullNames.Contains(
                         context.SemanticModel.GetSymbolInfo(bracketsNode.Parent).Symbol.ContainingSymbol.ToDisplayString()))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Evaluates if any of the .yml files match the specified function.
        /// </summary>
        /// <param name="files">The .yml files.</param>
        /// <param name="evaluateFunction">The evaluation function.</param>
        /// <param name="matchingFile">The matching file.</param>
        /// <returns></returns>
        private bool Evaluate(IEnumerable<AdditionalText> files, Func<RainbowFile, bool> evaluateFunction, out RainbowFile matchingFile)
        {
            matchingFile = null;
            foreach (AdditionalText text in files)
            {
                RainbowFile file;

                // This code will probably not run in parallel, but ensure that the dictionaries remain in sync just in case
                lock (this.rainbowFileHashes)
                {
                    if (this.rainbowFileHashes.ContainsKey(text.Path))
                    {
                        // We have already parsed this file, so check if our cache is up to date
                        if (this.rainbowFileHashes[text.Path] == text.GetHashCode())
                        {
                            // The cache is up to date, so use the cached version
                            file = this.rainbowFiles[text.Path];
                        }
                        else
                        {
                            // Update the cache
                            this.rainbowFileHashes[text.Path] = text.GetHashCode();
                            file = this.ParseRainbowFile(text);
                            this.rainbowFiles[text.Path] = file;
                        }
                    }
                    else
                    {
                        // Parse the file and add it to the cache
                        this.rainbowFileHashes.Add(text.Path, text.GetHashCode());
                        file = this.ParseRainbowFile(text);
                        this.rainbowFiles.Add(text.Path, file);
                    }
                }

                // Only stop evaluating if something matches
                if (evaluateFunction(file))
                {
                    matchingFile = file;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Parses the rainbow (.yml) file.
        /// </summary>
        /// <param name="text">The text of the .yml file.</param>
        /// <returns></returns>
        private RainbowFile ParseRainbowFile(AdditionalText text)
        {
            const string idParseKey = "ID: ";
            const string pathParseKey = "Path: ";
            const string templateParseKey = "Template: ";

            var textLines = text.GetText().ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (textLines.Any())
            {
                var result = new RainbowFile();

                string textLineId = textLines.FirstOrDefault(l => l.StartsWith(idParseKey));
                Guid idGuid;
                if (textLineId != null && Guid.TryParse(textLineId.Substring(idParseKey.Length).Trim(' ', '"'), out idGuid))
                {
                    result.Id = idGuid;
                }

                var textLinePath = textLines.FirstOrDefault(l => l.StartsWith(pathParseKey));
                if (textLinePath != null)
                {
                    result.Path = textLinePath.Substring(pathParseKey.Length).Trim();
                }

                string textLineTemplate = textLines.FirstOrDefault(l => l.StartsWith(templateParseKey));
                Guid templateGuid;
                if (textLineTemplate != null && Guid.TryParse(textLineTemplate.Substring(templateParseKey.Length).Trim(' ', '"'), out templateGuid))
                {
                    result.TemplateId = templateGuid;
                }

                return result;
            }

            // Not a valid result, but return an empty object anyway so it won't have to be parsed more often
            return new RainbowFile();
        }

    }
}
