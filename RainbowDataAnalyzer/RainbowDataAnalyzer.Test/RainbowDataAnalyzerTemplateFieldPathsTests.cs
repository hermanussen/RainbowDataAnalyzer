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
    public class RainbowDataAnalyzerTemplateFieldPathsTests : DiagnosticVerifier
    {
        [TestMethod]
        public void ShouldGetMessageAboutFieldWithFieldIndexer()
        {
            var expected = new DiagnosticResult
            {
                Id = "RainbowDataAnalyzerTemplateFieldPaths",
                Message = "The field 'Some field on template 2' is not on any of these templates: f5cfa142-fd92-4cdc-a6d5-c20020398418",
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 2, 144)
                        }
            };

            this.ExecuteTest("Some field on template 2", "f5cfa142-fd92-4cdc-a6d5-c20020398418", expected);
        }

        [TestMethod]
        public void ShouldNotGetMessageAboutFieldWithFieldIndexer()
        {
            this.ExecuteTest("Some field on template 1", "f5cfa142-fd92-4cdc-a6d5-c20020398418");
        }

        [TestMethod]
        public void ShouldNotGetMessageAboutFieldThroughInheritanceWithFieldIndexer()
        {
            this.ExecuteTest("Some field on template 1", "05cfa142-fd92-4cdc-a6d5-c20020398418");
        }

        private void ExecuteTest(string fieldName, string derivesFrom, params DiagnosticResult[] expected)
        {
            string test = string.Concat(
                "class TestClass { void SomeMethod() { var item = Sitecore.Context.Item.DerivesFrom(\"",
                derivesFrom,
                "\");var a = item.Fields[\"",
                fieldName,
                "\"]; } }");

            var yamlContents = new[]
            {
                "---\r\nID: \"f5cfa142-fd92-4cdc-a6d5-c20020398418\"\r\nParent: Path: /sitecore/templates/Some template 1\r\nTemplate: ab86861a-6030-46c5-b394-e8f99e8B87db",
                "---\r\nID: \"a5cfa142-fd92-4cdc-a6d5-c20020398418\"\r\nParent: Path: /sitecore/templates/Some template 2\r\nTemplate: ab86861a-6030-46c5-b394-e8f99e8B87db",
                "---\r\nID: \"05cfa142-fd92-4cdc-a6d5-c20020398418\"\r\nParent: Path: /sitecore/templates/Some template 1 1\r\nTemplate: ab86861a-6030-46c5-b394-e8f99e8B87db\r\nSharedFields:\r\n- ID: \"12c33f3f-86c5-43a5-aeb4-5598cec45116\"\r\n  Hint: __Base template\r\n  Type: tree list\r\n  Value: \"{F5CFA142-FD92-4CDC-A6D5-C20020398418}\"",
                "---\r\nID: \"38aa2f2c-7959-4bf6-9d26-42324904d9c8\"\r\nParent: f5cfa142-fd92-4cdc-a6d5-c20020398418",
                "---\r\nID: \"48aa2f2c-7959-4bf6-9d26-42324904d9c8\"\r\nParent: a5cfa142-fd92-4cdc-a6d5-c20020398418",
                "---\r\nID: \"58aa2f2c-7959-4bf6-9d26-42324904d9c8\"\r\nParent: 05cfa142-fd92-4cdc-a6d5-c20020398418",
                "---\r\nID: \"1ec9e41a-0d47-47ec-a0ac-2819edb60311\"\r\nParent: 38aa2f2c-7959-4bf6-9d26-42324904d9c8\r\nTemplate: 455a3e98-a627-4b40-8035-e683a0331ac7\r\nPath: /sitecore/templates/Some template 1/Some section/Some field on template 1",
                "---\r\nID: \"0ec9e41a-0d47-47ec-a0ac-2819edb60311\"\r\nParent: 48aa2f2c-7959-4bf6-9d26-42324904d9c8\r\nTemplate: 455a3e98-a627-4b40-8035-e683a0331ac7\r\nPath: /sitecore/templates/Some template 2/Some section/Some field on template 2",
                "---\r\nID: \"2ec9e41a-0d47-47ec-a0ac-2819edb60311\"\r\nParent: 58aa2f2c-7959-4bf6-9d26-42324904d9c8\r\nTemplate: 455a3e98-a627-4b40-8035-e683a0331ac7\r\nPath: /sitecore/templates/Some template 1 1/Some section/Some field on template 1 1"
            };

            this.VerifyCSharpDiagnostic(test, yamlContents, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new RainbowDataAnalyzerAnalyzer();
        }
    }
}