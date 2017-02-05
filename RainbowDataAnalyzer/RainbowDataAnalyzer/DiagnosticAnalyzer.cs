namespace RainbowDataAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using Constants;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Rainbow;

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

        private static DiagnosticDescriptor RuleForTemplateFieldIds = new DiagnosticDescriptor(
            "RainbowDataAnalyzerTemplateFieldIds",
            new LocalizableResourceString(nameof(Resources.TemplateFieldIdsAnalyzerTitle), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.TemplateFieldIdsAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources)),
            "Sitecore",
            DiagnosticSeverity.Error,
            true,
            new LocalizableResourceString(nameof(Resources.TemplateFieldIdsAnalyzerDescription), Resources.ResourceManager, typeof(Resources)));

        private static DiagnosticDescriptor RuleForTemplateFieldNames = new DiagnosticDescriptor(
            "RainbowDataAnalyzerTemplateFieldPaths",
            new LocalizableResourceString(nameof(Resources.TemplateFieldPathsAnalyzerTitle), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.TemplateFieldPathsAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources)),
            "Sitecore",
            DiagnosticSeverity.Error,
            true,
            new LocalizableResourceString(nameof(Resources.TemplateFieldPathsAnalyzerDescription), Resources.ResourceManager, typeof(Resources)));

        /// <summary>
        /// Rainbow file hashes that can be used to keep reference to caches for already parsed files.
        /// </summary>
        private readonly Dictionary<string, int> rainbowFileHashes = new Dictionary<string, int>();

        /// <summary>
        /// The read all files lock
        /// </summary>
        private readonly object readAllFilesLock = new object();

        /// <summary>
        /// Cached versions of parsed .yml files.
        /// </summary>
        private readonly Dictionary<string, RainbowFile> rainbowFiles = new Dictionary<string, RainbowFile>();

        /// <summary>
        /// Returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(RuleForIds, RuleForPaths, RuleForFieldIds, RuleForFieldNames, RuleForTemplateFieldIds, RuleForTemplateFieldNames); } }

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
            List<Guid> allowedTemplateIds;

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
                    if (context.Node.Ancestors().Any(n =>
                             n is AttributeSyntax
                             && nameof(Guid).Equals(((AttributeSyntax)n).Name.ToString(), StringComparison.OrdinalIgnoreCase)))
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

                    if (validateAsField)
                    {
                        if(matchingFile.TemplateId != SitecoreConstants.SitecoreTemplateFieldId)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(RuleForFieldIds, context.Node.GetLocation(), pathOrId));
                        }
                        else if(this.ViolatesTemplate(context, matchingFile.Id, out allowedTemplateIds))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(RuleForTemplateFieldIds, context.Node.GetLocation(), pathOrId, string.Join(", ", allowedTemplateIds)));
                        }
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
                    if (!this.Evaluate(context.Options.AdditionalFiles, file => file.TemplateId == SitecoreConstants.SitecoreTemplateFieldId
                                                                                && string.Equals(file.ItemName, pathOrId, StringComparison.OrdinalIgnoreCase), out matchingFile))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(RuleForFieldNames, context.Node.GetLocation(), pathOrId));
                    }
                    else if (this.ViolatesTemplate(context, matchingFile.Id, out allowedTemplateIds))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(RuleForTemplateFieldNames, context.Node.GetLocation(), pathOrId, string.Join(", ", allowedTemplateIds)));
                    }
                }
            }
            finally
            {
                Debug.WriteLine($"Analyzed syntax node action in {(DateTime.Now - startTime).Milliseconds}ms");
            }
        }

        private bool ViolatesTemplate(SyntaxNodeAnalysisContext context, Guid fieldId, out List<Guid> allowedTemplateIds)
        {
            allowedTemplateIds = new List<Guid>();
            // We know that we are dealing with an existing template field; now we should find out if it can be limited to a particular template
            var method = context.Node.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (method == null)
            {
                return false;
            }

            var derives = method.DescendantNodes().OfType<InvocationExpressionSyntax>()
                .Where(i => string.Equals("DerivesFrom", i.ChildNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()?.Name?.ToString()));
            foreach (var derive in derives)
            {
                string pathOrIdArg = derive.ArgumentList.Arguments.LastOrDefault()?.DescendantTokens()
                    .FirstOrDefault(t => t.Kind() == SyntaxKind.StringLiteralToken).ValueText;
                if (!string.IsNullOrWhiteSpace(pathOrIdArg))
                {
                    Guid templateId;
                    if (Guid.TryParse(pathOrIdArg, out templateId))
                    {
                        if (!this.FindAllTemplatesForField(context.Options.AdditionalFiles, fieldId).Contains(templateId))
                        {
                            allowedTemplateIds.Add(templateId);
                            return true;
                        }
                    }
                    else if (pathOrIdArg.StartsWith("/sitecore/", StringComparison.OrdinalIgnoreCase))
                    {
                        RainbowFile matchingFile;
                        if (!this.Evaluate(context.Options.AdditionalFiles, file => string.Equals(file.Path, pathOrIdArg.TrimEnd('/'), StringComparison.OrdinalIgnoreCase), out matchingFile))
                        {
                            if (!this.FindAllTemplatesForField(context.Options.AdditionalFiles, fieldId).Contains(matchingFile.Id))
                            {
                                allowedTemplateIds.Add(matchingFile.Id);
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if the syntax node likely refers to a field ID or path.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static bool ShouldValidateAsField(SyntaxNodeAnalysisContext context)
        {
            var bracketNodes = context.Node.Ancestors()
                .Where(a => a is BracketedArgumentListSyntax || a is ArgumentListSyntax);

            return bracketNodes.Any(b => b.Parent.ChildNodes().First().ChildNodes()
                .LastOrDefault(c => c is IdentifierNameSyntax && SitecoreConstants.IdentifierSyntaxNames.Contains(c.ToString())) != null);
        }

        /// <summary>
        /// Finds all templates for a field ID.
        /// </summary>
        /// <param name="files">The files.</param>
        /// <param name="fieldId">The field identifier.</param>
        /// <returns></returns>
        private IEnumerable<Guid> FindAllTemplatesForField(IEnumerable<AdditionalText> files, Guid fieldId)
        {
            List<Guid> result = new List<Guid>();
            lock (this.readAllFilesLock)
            {
                RainbowFile file;
                this.Evaluate(files, null, out file);

                var allRainbowFiles = this.rainbowFiles.Values;
                RainbowFile potentialTemplateFile = allRainbowFiles.FirstOrDefault(f => Guid.Equals(f.Id, fieldId));
                while (potentialTemplateFile != null)
                {
                    if (Guid.Equals(potentialTemplateFile.TemplateId, SitecoreConstants.SitecoreTemplateTemplateId))
                    {
                        break;
                    }

                    potentialTemplateFile = allRainbowFiles.FirstOrDefault(f => Guid.Equals(f.Id, potentialTemplateFile.ParentId));
                }

                if (potentialTemplateFile != null)
                {
                    result.Add(potentialTemplateFile.Id);

                    var allTemplateFiles = allRainbowFiles.Where(f => Guid.Equals(f.TemplateId, SitecoreConstants.SitecoreTemplateTemplateId)).ToList();
                    result.AddRange(FindDerivedTemplates(allTemplateFiles, potentialTemplateFile.Id, 200));
                }
            }

            return result.Distinct();
        }

        /// <summary>
        /// Finds derived templates recursively.
        /// </summary>
        /// <param name="allTemplateFiles">All template files.</param>
        /// <param name="templateId">The template ID.</param>
        /// <param name="maxDepth">The maximum depth, to prevent stack overflow if there are circular references.</param>
        /// <returns></returns>
        private static IEnumerable<Guid> FindDerivedTemplates(List<RainbowFile> allTemplateFiles, Guid templateId, int maxDepth)
        {
            if (maxDepth > 0)
            {
                foreach (Guid derivedTemplateId in allTemplateFiles.Where(t => t.BaseTemplates != null && t.BaseTemplates.Contains(templateId)).Select(t => t.Id))
                {
                    yield return derivedTemplateId;

                    foreach (var derived in FindDerivedTemplates(allTemplateFiles, derivedTemplateId, maxDepth - 1))
                    {
                        yield return derived;
                    }
                }
            }
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
                            file = RainbowParserUtil.ParseRainbowFile(text);
                            this.rainbowFiles[text.Path] = file;
                        }
                    }
                    else
                    {
                        // Parse the file and add it to the cache
                        this.rainbowFileHashes.Add(text.Path, text.GetHashCode());
                        file = RainbowParserUtil.ParseRainbowFile(text);
                        this.rainbowFiles.Add(text.Path, file);
                    }
                }

                // Only stop evaluating if something matches
                if (evaluateFunction != null && evaluateFunction(file))
                {
                    matchingFile = file;
                    return true;
                }
            }

            return false;
        }
    }
}
