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

        private static DiagnosticDescriptor RuleForInfoIdToPath = new DiagnosticDescriptor(
            "RainbowDataAnalyzerIdToPath",
            new LocalizableResourceString(nameof(Resources.RainbowDataAnalyzerIdToPathTitle), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.RainbowDataAnalyzerIdToPathMessageFormat), Resources.ResourceManager, typeof(Resources)),
            "Sitecore",
            DiagnosticSeverity.Info,
            true,
            new LocalizableResourceString(nameof(Resources.RainbowDataAnalyzerIdToPathDescription), Resources.ResourceManager, typeof(Resources)));

        private static DiagnosticDescriptor RuleForInfoPathToId = new DiagnosticDescriptor(
            "RainbowDataAnalyzerPathToId",
            new LocalizableResourceString(nameof(Resources.RainbowDataAnalyzerPathToIdTitle), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.RainbowDataAnalyzerPathToIdMessageFormat), Resources.ResourceManager, typeof(Resources)),
            "Sitecore",
            DiagnosticSeverity.Info,
            true,
            new LocalizableResourceString(nameof(Resources.RainbowDataAnalyzerPathToIdDescription), Resources.ResourceManager, typeof(Resources)));
        
        /// <summary>
        /// The names of possible Sitecore item types.
        /// </summary>
        private static readonly string[] sitecoreItemFullNames =
            {
                "Sitecore.Data.Items.BaseItem",
                "Sitecore.Data.Items.Item"
            };

        /// <summary>
        /// The names of Glass attribute types that represent a field
        /// </summary>
        private static readonly string[] glassFieldAttributeNames =
            {
                "Glass.Mapper.Sc.Configuration.Attributes.SitecoreFieldAttribute"
            };

        /// <summary>
        /// The names of Glass attribute types that represent a type
        /// </summary>
        private static readonly string[] glassTypeAttributeNames =
            {
                "Glass.Mapper.Sc.Configuration.Attributes.SitecoreTypeAttribute"
            };

        /// <summary>
        /// Method names that can be used to indicate that an item template is derived from a specific template
        /// </summary>
        private static readonly string[] derivedMethodNames =
            {
                "MustDeriveFrom",
                "IsDerived"
            };

        /// <summary>
        /// Returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(RuleForIds, RuleForPaths, RuleForFieldIds, RuleForFieldNames, RuleForTemplateFieldIds, RuleForTemplateFieldNames, RuleForInfoIdToPath, RuleForInfoPathToId); } }

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
            string allowedTemplate;

            try
            {
                // If there are no .yml files available in the project, we should not validate
                if (!context.Options.AdditionalFiles.Any())
                {
                    return;
                }

                // If this ID/path refers to a field, we should apply more strict validation
                bool validateAsField = ShouldValidateAsField(context);
                bool validateAsGlassField = ShouldValidateAsGlassField(context);

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
                    if (!Repository.Evaluate(context.Options.AdditionalFiles, file => Guid.Equals(file.Id, valueAsId), out matchingFile))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(RuleForIds, context.Node.GetLocation(), pathOrId));
                        return;
                    }

                    bool reportInfo = true;
                    if (validateAsField || validateAsGlassField)
                    {
                        if(matchingFile.TemplateId != SitecoreConstants.SitecoreTemplateFieldId)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(RuleForFieldIds, context.Node.GetLocation(), pathOrId));
                            reportInfo = false; // there is already something reported
                        }
                        else if((validateAsField && this.ViolatesTemplate(context, matchingFile.Id, out allowedTemplate))
                             || (validateAsGlassField && this.ViolatesGlassTemplate(context, matchingFile.Id, out allowedTemplate)))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(RuleForTemplateFieldIds, context.Node.GetLocation(), pathOrId, allowedTemplate));
                            reportInfo = false; // there is already something reported
                        }
                    }

                    if (reportInfo)
                    {
                        // The item was found; offer a hint to the path
                        context.ReportDiagnostic(Diagnostic.Create(RuleForInfoIdToPath, context.Node.GetLocation(), matchingFile.Path));
                    }
                }
                else if (pathOrId.StartsWith("/sitecore/", StringComparison.OrdinalIgnoreCase))
                {
                    // All paths that start with /sitecore/ are regarded as Sitecore paths and will be checked
                    RainbowFile matchingFile;
                    if (!Repository.Evaluate(context.Options.AdditionalFiles, file => string.Equals(file.Path, pathOrId.TrimEnd('/'), StringComparison.OrdinalIgnoreCase), out matchingFile))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(RuleForPaths, context.Node.GetLocation(), pathOrId));
                    }
                    else
                    {
                        // The item was found; offer a hint to the ID
                        context.ReportDiagnostic(Diagnostic.Create(RuleForInfoPathToId, context.Node.GetLocation(), matchingFile.Id));
                    }
                }
                else if (validateAsField && !pathOrId.Contains("/"))
                {
                    RainbowFile matchingFile;
                    if (!Repository.Evaluate(context.Options.AdditionalFiles, file => file.TemplateId == SitecoreConstants.SitecoreTemplateFieldId
                                                                                && string.Equals(file.ItemName, pathOrId, StringComparison.OrdinalIgnoreCase), out matchingFile))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(RuleForFieldNames, context.Node.GetLocation(), pathOrId));
                    }
                    else if (this.ViolatesTemplate(context, matchingFile.Id, out allowedTemplate))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(RuleForTemplateFieldNames, context.Node.GetLocation(), pathOrId, allowedTemplate));
                    }
                }
            }
            finally
            {
                Debug.WriteLine($"Analyzed syntax node action in {(DateTime.Now - startTime).Milliseconds}ms");
            }
        }

        private bool ViolatesTemplate(SyntaxNodeAnalysisContext context, Guid fieldId, out string allowedTemplate)
        {
            allowedTemplate = null;

            // We know that we are dealing with an existing template field; now we should find out if it can be limited to a particular template
            var method = context.Node.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (method == null)
            {
                return false;
            }

            var derives = method.DescendantNodes().OfType<InvocationExpressionSyntax>()
                .Where(i => derivedMethodNames.Contains(i.ChildNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()?.Name?.ToString())
                         || derivedMethodNames.Contains(i.ChildNodes().OfType<MemberBindingExpressionSyntax>().FirstOrDefault()?.Name?.ToString()));
            foreach (var derive in derives)
            {
                string objectName;
                if (derive.Parent is ConditionalAccessExpressionSyntax)
                {
                    objectName = derive.Parent.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault()?.Identifier.ValueText;
                }
                else
                {
                    objectName = derive.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault()?.Identifier.ValueText;
                }

                string itemObjectName = context.Node.Ancestors().OfType<ElementAccessExpressionSyntax>().FirstOrDefault()?.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault()?.Identifier.ValueText;

                // Check if it is the same object
                if (!string.Equals(objectName, itemObjectName))
                {
                    continue;
                }

                string pathOrIdArg = derive.ArgumentList.Arguments.LastOrDefault()?.DescendantTokens()
                    .FirstOrDefault(t => t.Kind() == SyntaxKind.StringLiteralToken).ValueText;
                if (!string.IsNullOrWhiteSpace(pathOrIdArg))
                {
                    Guid templateId;
                    if (Guid.TryParse(pathOrIdArg, out templateId))
                    {
                        var allTemplatesForField = Repository.FindAllTemplatesForField(context.Options.AdditionalFiles, fieldId);
                        
                        if (!allTemplatesForField.Select(t => t.Id).Contains(templateId))
                        {
                            RainbowFile matchingFile;
                            Repository.Evaluate(context.Options.AdditionalFiles, file => file.Id == templateId, out matchingFile);

                            allowedTemplate = $"{matchingFile.Id} ({matchingFile.ItemName})";
                            return true;
                        }
                    }
                    else if (pathOrIdArg.StartsWith("/sitecore/", StringComparison.OrdinalIgnoreCase))
                    {
                        RainbowFile matchingFile;
                        if (!Repository.Evaluate(context.Options.AdditionalFiles, file => string.Equals(file.Path, pathOrIdArg.TrimEnd('/'), StringComparison.OrdinalIgnoreCase), out matchingFile))
                        {
                            var allTemplatesForField = Repository.FindAllTemplatesForField(context.Options.AdditionalFiles, fieldId);
                            
                            if (!allTemplatesForField.Select(t => t.Id).Contains(matchingFile.Id))
                            {
                                allowedTemplate = $"{matchingFile.Id} ({matchingFile.ItemName})";
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private bool ViolatesGlassTemplate(SyntaxNodeAnalysisContext context, Guid fieldId, out string allowedTemplate)
        {
            allowedTemplate = null;

            // We know that we are dealing with an existing template field; now we should find out if it can be limited to a particular template
            var type = context.Node.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
            if (type == null)
            {
                return false;
            }

            string templateIdStr = type.ChildNodes().OfType<AttributeListSyntax>()
                .SelectMany(l => l.Attributes.Where(a => glassTypeAttributeNames.Contains(context.SemanticModel.GetSymbolInfo(a).Symbol?.ContainingSymbol.ToDisplayString())))
                .SelectMany(a => a.DescendantNodes().OfType<AttributeArgumentSyntax>())
                .Where(a => "TemplateId".Equals(a.NameEquals.Name.Identifier.ValueText) && a.Expression is LiteralExpressionSyntax)
                .Select(a => ((LiteralExpressionSyntax) a.Expression).Token.ValueText)
                .FirstOrDefault();
            Guid templateId;
            if (Guid.TryParse(templateIdStr, out templateId))
            {
                var allTemplatesForField = Repository.FindAllTemplatesForField(context.Options.AdditionalFiles, fieldId);

                if (!allTemplatesForField.Select(t => t.Id).Contains(templateId))
                {
                    RainbowFile matchingFile;
                    Repository.Evaluate(context.Options.AdditionalFiles, file => file.Id == templateId, out matchingFile);

                    allowedTemplate = $"{matchingFile.Id} ({matchingFile.ItemName})";
                    return true;
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
            
            // If this is an invocation like item["field"], then it counts as a field
            if (bracketNodes.Select(b => b.Parent).OfType<ElementAccessExpressionSyntax>()
                    .Any(e => sitecoreItemFullNames.Contains(context.SemanticModel.GetSymbolInfo(e).Symbol?.ContainingSymbol.ToDisplayString())))
            {
                return true;
            }

            bool isMethodForFieldRendering = bracketNodes.Any(b => b.Parent.ChildNodes().First().ChildNodes()
                .LastOrDefault(c => c is IdentifierNameSyntax && SitecoreConstants.IdentifierSyntaxNames.Contains(c.ToString())) != null);
            return isMethodForFieldRendering && bracketNodes.All(b => b.ChildNodes().FirstOrDefault()?.DescendantNodes().Contains(context.Node) ?? false);
        }

        /// <summary>
        /// Returns true if the syntax node likely refers to a field ID or path based on a Glass attribute.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static bool ShouldValidateAsGlassField(SyntaxNodeAnalysisContext context)
        {
            var attribute = context.Node.Ancestors().FirstOrDefault(a => a is AttributeSyntax);
            if (attribute != null)
            {
                string attributeTypeName = context.SemanticModel.GetSymbolInfo(attribute).Symbol?.ContainingSymbol.ToDisplayString();
                if (attributeTypeName != null && glassFieldAttributeNames.Contains(attributeTypeName))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
