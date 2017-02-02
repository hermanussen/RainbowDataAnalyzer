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
    public class RainbowDataAnalyzerFieldIdsTests : DiagnosticVerifier
    {
        [TestMethod]
        public void ShouldGetMessageAboutFieldWithFieldIndexer()
        {
            var test = @"class TestClass { void SomeMethod() { var a = Sitecore.Context.Item.Fields[new Sitecore.Data.ID(""0ec9e41a-0d47-47ec-a0ac-2819edb60311"")]; } }";

            var yamlContents = new string[] {
                "---\r\nID: \"0ec9e41a-0d47-47ec-a0ac-2819edb60311\"\r\nPath: /sitecore/existent"
            };

            var expected = new DiagnosticResult
            {
                Id = "RainbowDataAnalyzerFieldIds",
                Message = "This ID refers to a Sitecore item which is not a field (checked in Rainbow data)",
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 97)
                        }
            };

            this.VerifyCSharpDiagnostic(test, yamlContents, expected);
        }

        [TestMethod]
        public void ShouldNotGetMessageAboutFieldWithFieldIndexer()
        {
            var test = @"class TestClass { void SomeMethod() { var a = Sitecore.Context.Item.Fields[new Sitecore.Data.ID(""0ec9e41a-0d47-47ec-a0ac-2819edb60311"")]; } }";

            var yamlContents = new string[] {
                "---\r\nID: \"0ec9e41a-0d47-47ec-a0ac-2819edb60311\"\r\nPath: /sitecore/existent\r\nTemplate: 455a3e98-a627-4b40-8035-e683a0331ac7"
            };

            this.VerifyCSharpDiagnostic(test, yamlContents);
        }

        [TestMethod]
        public void ShouldGetMessageAboutFieldWithItem()
        {
            var test = @"class TestClass { void SomeMethod() { var a = Sitecore.Context.Item[new Sitecore.Data.ID(""0ec9e41a-0d47-47ec-a0ac-2819edb60311"")]; } }";

            var yamlContents = new string[] {
                "---\r\nID: \"0ec9e41a-0d47-47ec-a0ac-2819edb60311\"\r\nPath: /sitecore/existent"
            };

            var expected = new DiagnosticResult
            {
                Id = "RainbowDataAnalyzerFieldIds",
                Message = "This ID refers to a Sitecore item which is not a field (checked in Rainbow data)",
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 90)
                        }
            };

            this.VerifyCSharpDiagnostic(test, yamlContents, expected);
        }

        [TestMethod]
        public void ShouldNotGetMessageAboutFieldWithItem()
        {
            var test = @"class TestClass { void SomeMethod() { var a = Sitecore.Context.Item[new Sitecore.Data.ID(""0ec9e41a-0d47-47ec-a0ac-2819edb60311"")]; } }";

            var yamlContents = new string[] {
                "---\r\nID: \"0ec9e41a-0d47-47ec-a0ac-2819edb60311\"\r\nPath: /sitecore/existent\r\nTemplate: 455a3e98-a627-4b40-8035-e683a0331ac7"
            };

            this.VerifyCSharpDiagnostic(test, yamlContents);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new RainbowDataAnalyzerAnalyzer();
        }
    }
}