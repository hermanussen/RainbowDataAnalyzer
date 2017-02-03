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
    public class RainbowDataAnalyzerFieldPathsTests : DiagnosticVerifier
    {
        [TestMethod]
        public void ShouldGetMessageAboutFieldWithFieldIndexer()
        {
            var test = @"class TestClass { void SomeMethod() { var a = Sitecore.Context.Item.Fields[""Some field that does not exist""]; } }";

            var yamlContents = new []
                {
                    "---\r\nID: \"0ec9e41a-0d47-47ec-a0ac-2819edb60311\"\r\nPath: /sitecore/templates/Some template/Some section/Some field"
                };

            var expected = new DiagnosticResult
            {
                Id = "RainbowDataAnalyzerFieldPaths",
                Message = "A valid template field with name 'Some field that does not exist' could not be found",
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 76)
                        }
            };

            this.VerifyCSharpDiagnostic(test, yamlContents, expected);
        }

        [TestMethod]
        public void ShouldNotGetMessageAboutFieldWithFieldIndexer()
        {
            var test = @"class TestClass { void SomeMethod() { var a = Sitecore.Context.Item.Fields[""Some field""]; } }";

            var yamlContents = new []
                {
                    "---\r\nID: \"0ec9e41a-0d47-47ec-a0ac-2819edb60311\"\r\nPath: /sitecore/templates/Some template/Some section/Some field\r\nTemplate: 455a3e98-a627-4b40-8035-e683a0331ac7"
                };

            this.VerifyCSharpDiagnostic(test, yamlContents);
        }

        [TestMethod]
        public void ShouldGetMessageAboutFieldWithItem()
        {
            var test = @"class TestClass { void SomeMethod() { var a = Sitecore.Context.Item[""Some field that does not exist""]; } }";

            var yamlContents = new[]
                {
                    "---\r\nID: \"0ec9e41a-0d47-47ec-a0ac-2819edb60311\"\r\nPath: /sitecore/templates/Some template/Some section/Some field"
                };

            var expected = new DiagnosticResult
            {
                Id = "RainbowDataAnalyzerFieldPaths",
                Message = "A valid template field with name 'Some field that does not exist' could not be found",
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 69)
                        }
            };

            this.VerifyCSharpDiagnostic(test, yamlContents, expected);
        }

        [TestMethod]
        public void ShouldNotGetMessageAboutFieldWithItem()
        {
            var test = @"class TestClass { void SomeMethod() { var a = Sitecore.Context.Item[""Some field""]; } }";

            var yamlContents = new[]
                {
                    "---\r\nID: \"0ec9e41a-0d47-47ec-a0ac-2819edb60311\"\r\nPath: /sitecore/templates/Some template/Some section/Some field\r\nTemplate: 455a3e98-a627-4b40-8035-e683a0331ac7"
                };

            this.VerifyCSharpDiagnostic(test, yamlContents);
        }

        [TestMethod]
        public void ShouldGetMessageAboutFieldWithHtmlHelper()
        {
            var test = "using Sitecore.Mvc;\r\nclass TestClass { void SomeMethod() { new System.Web.Mvc.HtmlHelper(null, null).Sitecore().Field(\"Some field that does not exist\"); } }";

            var yamlContents = new[]
                {
                    "---\r\nID: \"0ec9e41a-0d47-47ec-a0ac-2819edb60311\"\r\nPath: /sitecore/templates/Some template/Some section/Some field"
                };

            var expected = new DiagnosticResult
            {
                Id = "RainbowDataAnalyzerFieldPaths",
                Message = "A valid template field with name 'Some field that does not exist' could not be found",
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 2, 98)
                        }
            };

            this.VerifyCSharpDiagnostic(test, yamlContents, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new RainbowDataAnalyzerAnalyzer();
        }
    }
}