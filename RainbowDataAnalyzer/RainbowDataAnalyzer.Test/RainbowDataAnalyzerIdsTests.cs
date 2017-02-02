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
    public class RainbowDataAnalyzerIdsTests : DiagnosticVerifier
    {
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

            var yamlContents = new[] {
                "---\r\nID: \"0ec9e41a-0d47-47ec-a0ac-2819edb60311\"\r\nPath: /sitecore/existent"
            };

            this.VerifyCSharpDiagnostic(test, yamlContents, expected);
        }
        
        [TestMethod]
        public void ShouldNotGetMessageAboutId()
        {
            var test = "class TestClass { string scId = \"{0ec9e41a-0d47-47ec-a0ac-2819edb60311}\"; }";
            
            var yamlContents = new[] {
                "---\r\nID: \"0ec9e41a-0d47-47ec-a0ac-2819edb60311\"\r\nPath: /sitecore/existent"
            };

            this.VerifyCSharpDiagnostic(test, yamlContents);
        }
        
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new RainbowDataAnalyzerAnalyzer();
        }
    }
}