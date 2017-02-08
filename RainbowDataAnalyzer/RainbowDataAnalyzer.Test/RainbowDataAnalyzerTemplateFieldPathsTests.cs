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
        public void AnimalsDoNotHaveSteeringWheels()
        {
            var expected = new DiagnosticResult
            {
                Id = "RainbowDataAnalyzerTemplateFieldPaths",
                Message = "The field 'Steering wheel' is not on the template 'f5cfa142-fd92-4cdc-a6d5-c20020398418 (Animal template)' or on any of its base templates",
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 2, 152)
                        }
            };
            
            string source = string.Concat(
                "class TestClass { void SomeMethod() { var item = Sitecore.Context.Item;item.MustDeriveFrom(\"",
                "f5cfa142-fd92-4cdc-a6d5-c20020398418",
                "\");var a = item.Fields[\"",
                "Steering wheel",
                "\"]; } }");

            this.ExecuteTest(source, expected);
        }

        [TestMethod]
        public void AnimalsHaveFoodThatTheyLike()
        {
            string source = string.Concat(
                "class TestClass { void SomeMethod() { var item = Sitecore.Context.Item;item.MustDeriveFrom(\"",
                "f5cfa142-fd92-4cdc-a6d5-c20020398418",
                "\");var a = item.Fields[\"",
                "Food that it likes",
                "\"]; } }");

            this.ExecuteTest(source);
        }

        [TestMethod]
        public void AnimalsDoNotAlwaysHaveCatHair()
        {
            var expected = new DiagnosticResult
                {
                    Id = "RainbowDataAnalyzerTemplateFieldPaths",
                    Message = "The field 'Cat hair' is not on the template 'f5cfa142-fd92-4cdc-a6d5-c20020398418 (Animal template)' or on any of its base templates",
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 2, 152)
                            }
                };

            string source = string.Concat(
                "class TestClass { void SomeMethod() { var item = Sitecore.Context.Item;item.MustDeriveFrom(\"",
                "f5cfa142-fd92-4cdc-a6d5-c20020398418",
                "\");var a = item.Fields[\"",
                "Cat hair",
                "\"]; } }");

            this.ExecuteTest(source, expected);
        }
        
        [TestMethod]
        public void AnimalsAndOtherOrganismsDoNotAlwaysHaveCatHair()
        {
            var expected = new DiagnosticResult
            {
                Id = "RainbowDataAnalyzerTemplateFieldPaths",
                Message = "The field 'Cat hair' is not on the template 'f5cfa142-fd92-4cdc-a6d5-c20020398418 (Animal template)' or on any of its base templates",
                Severity = DiagnosticSeverity.Error,
                Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 2, 152)
                            }
            };

            string source = string.Concat(
                "class TestClass { void SomeMethod() { var item = Sitecore.Context.Item;item.MustDeriveFrom(\"",
                "f5cfa142-fd92-4cdc-a6d5-c20020398418",
                "\");var a = item.Fields[\"",
                "Cat hair",
                "\"]; } }");

            this.ExecuteTest(source, expected);
        }

        [TestMethod]
        public void CatsHaveFoodThatTheyLike()
        {
            string source = string.Concat(
                "class TestClass { void SomeMethod() { var item = Sitecore.Context.Item;item.MustDeriveFrom(\"",
                "05cfa142-fd92-4cdc-a6d5-c20020398418",
                "\");var a = item.Fields[\"",
                "Food that it likes",
                "\"]; } }");

            this.ExecuteTest(source);
        }

        [TestMethod]
        public void AnimalsDoNotHaveSteeringWheelsButThatIsNotWhatIsBeingChecked()
        {
            string source = string.Concat(
                "class TestClass { void SomeMethod() { var item = Sitecore.Context.Item;var item2 = Sitecore.Context.Database.GetRootItem();item2.MustDeriveFrom(\"",
                "f5cfa142-fd92-4cdc-a6d5-c20020398418",
                "\");var a = item.Fields[\"",
                "Steering wheel",
                "\"]; } }");

            this.ExecuteTest(source);
        }

        private void ExecuteTest(string source, params DiagnosticResult[] expected)
        {
            var yamlContents = new[]
            {
                "---\r\nID: \"15cfa142-fd92-4cdc-a6d5-c20020398418\"\r\nPath: /sitecore/templates/Organism template\r\nTemplate: ab86861a-6030-46c5-b394-e8f99e8B87db",
                "---\r\nID: \"f5cfa142-fd92-4cdc-a6d5-c20020398418\"\r\nPath: /sitecore/templates/Animal template\r\nTemplate: ab86861a-6030-46c5-b394-e8f99e8B87db\r\nSharedFields:\r\n- ID: \"12c33f3f-86c5-43a5-aeb4-5598cec45116\"\r\n  Hint: __Base template\r\n  Type: tree list\r\n  Value: \"{15CFA142-FD92-4CDC-A6D5-C20020398418}\"",
                "---\r\nID: \"a5cfa142-fd92-4cdc-a6d5-c20020398418\"\r\nPath: /sitecore/templates/Bicycle template\r\nTemplate: ab86861a-6030-46c5-b394-e8f99e8B87db",
                "---\r\nID: \"05cfa142-fd92-4cdc-a6d5-c20020398418\"\r\nPath: /sitecore/templates/Cat template\r\nTemplate: ab86861a-6030-46c5-b394-e8f99e8B87db\r\nSharedFields:\r\n- ID: \"12c33f3f-86c5-43a5-aeb4-5598cec45116\"\r\n  Hint: __Base template\r\n  Type: tree list\r\n  Value: \"{F5CFA142-FD92-4CDC-A6D5-C20020398418}\"",
                "---\r\nID: \"38aa2f2c-7959-4bf6-9d26-42324904d9c8\"\r\nParent: f5cfa142-fd92-4cdc-a6d5-c20020398418",
                "---\r\nID: \"48aa2f2c-7959-4bf6-9d26-42324904d9c8\"\r\nParent: a5cfa142-fd92-4cdc-a6d5-c20020398418",
                "---\r\nID: \"58aa2f2c-7959-4bf6-9d26-42324904d9c8\"\r\nParent: 05cfa142-fd92-4cdc-a6d5-c20020398418",
                "---\r\nID: \"1ec9e41a-0d47-47ec-a0ac-2819edb60311\"\r\nParent: 38aa2f2c-7959-4bf6-9d26-42324904d9c8\r\nTemplate: 455a3e98-a627-4b40-8035-e683a0331ac7\r\nPath: /sitecore/templates/Animal template/Some section/Food that it likes",
                "---\r\nID: \"0ec9e41a-0d47-47ec-a0ac-2819edb60311\"\r\nParent: 48aa2f2c-7959-4bf6-9d26-42324904d9c8\r\nTemplate: 455a3e98-a627-4b40-8035-e683a0331ac7\r\nPath: /sitecore/templates/Bicycle template/Some section/Steering wheel",
                "---\r\nID: \"2ec9e41a-0d47-47ec-a0ac-2819edb60311\"\r\nParent: 58aa2f2c-7959-4bf6-9d26-42324904d9c8\r\nTemplate: 455a3e98-a627-4b40-8035-e683a0331ac7\r\nPath: /sitecore/templates/Cat template/Some section/Cat hair"
            };

            this.VerifyCSharpDiagnostic(source, yamlContents, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new RainbowDataAnalyzerAnalyzer();
        }
    }
}