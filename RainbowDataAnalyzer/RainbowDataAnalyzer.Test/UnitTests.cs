namespace RainbowDataAnalyzer.Test
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using TestHelper;
    using RainbowDataAnalyzer;

    [TestClass]
    public class UnitTest : CodeFixVerifier
    {
        
        [TestMethod]
        public void ShouldNotGetMessages()
        {
            var test = @"";

            this.VerifyCSharpDiagnostic(test);
        }
        
        [TestMethod]
        public void ShouldGetMessageAboutId()
        {
            var test = "class TestClass { string scId = \"{aec9e41a-0d47-47ec-a0ac-2819edb60311}\"; }";
            var expected = new DiagnosticResult
            {
                Id = "RainbowDataAnalyzerIds",
                Message = "An item with id '{aec9e41a-0d47-47ec-a0ac-2819edb60311}' could not be found",
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 33)
                        }
            };

            this.VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void ShouldGetMessageAboutPath()
        {
            var test = "class TestClass { string scPath = \"/sitecore/nonexistent\"; }";
            var expected = new DiagnosticResult
            {
                Id = "RainbowDataAnalyzerPaths",
                Message = "An item with path '/sitecore/nonexistent' could not be found",
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 35)
                        }
            };

            this.VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new RainbowDataAnalyzerAnalyzer();
        }
    }
}