namespace RainbowDataAnalyzer.Test
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelper;

    [TestClass]
    public class RainbowDataAnalyzerTemplateFieldIdsGlassTests : DiagnosticVerifier
    {
        [TestMethod]
        public void AnimalsDoNotHaveSteeringWheels()
        {
            var expectedInfo = new DiagnosticResult
            {
                Id = "RainbowDataAnalyzerIdToPath",
                Message = "The ID corresponds with path '/sitecore/templates/Animal template'",
                Severity = DiagnosticSeverity.Info,
                Locations =
                                        new[] {
                                                new DiagnosticResultLocation("Test0.cs", 11, 69)
                                            }
            };

            var expected = new DiagnosticResult
            {
                Id = "RainbowDataAnalyzerTemplateFieldIds",
                Message = "The field '{0ec9e41a-0d47-47ec-a0ac-2819edb60311}' is not on the template 'f5cfa142-fd92-4cdc-a6d5-c20020398418 (Animal template)' or on any of its base templates",
                Severity = DiagnosticSeverity.Error,
                Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 2, 193)
                            }
            };

            string source = "[Glass.Mapper.Sc.Configuration.Attributes.SitecoreType(TemplateId = \"{f5cfa142-fd92-4cdc-a6d5-c20020398418}\")] class Animal { [Glass.Mapper.Sc.Configuration.Attributes.SitecoreField(FieldId = \"{0ec9e41a-0d47-47ec-a0ac-2819edb60311}\")] string SteeringWheel { get;set; } }";

            this.ExecuteTest(source, expectedInfo, expected);
        }

        [TestMethod]
        public void AnimalsHaveFoodThatTheyLike()
        {
            var expectedInfo = new DiagnosticResult
            {
                Id = "RainbowDataAnalyzerIdToPath",
                Message = "The ID corresponds with path '/sitecore/templates/Animal template'",
                Severity = DiagnosticSeverity.Info,
                Locations =
                                                    new[] {
                                                            new DiagnosticResultLocation("Test0.cs", 11, 69)
                                                        }
            };

            var expectedInfo2 = new DiagnosticResult
            {
                Id = "RainbowDataAnalyzerIdToPath",
                Message = "The ID corresponds with path '/sitecore/templates/Animal template/Some section/Food that it likes'",
                Severity = DiagnosticSeverity.Info,
                Locations =
                                                        new[] {
                                                                new DiagnosticResultLocation("Test0.cs", 11, 193)
                                                            }
            };

            string source = "[Glass.Mapper.Sc.Configuration.Attributes.SitecoreType(TemplateId = \"{f5cfa142-fd92-4cdc-a6d5-c20020398418}\")] class Animal { [Glass.Mapper.Sc.Configuration.Attributes.SitecoreField(FieldId = \"{1ec9e41a-0d47-47ec-a0ac-2819edb60311}\")] string FoodThatItLikes { get;set; } }";

            this.ExecuteTest(source, expectedInfo, expectedInfo2);
        }

        [TestMethod]
        public void AnimalsDoNotAlwaysHaveCatHair()
        {
            var expectedInfo = new DiagnosticResult
            {
                Id = "RainbowDataAnalyzerIdToPath",
                Message = "The ID corresponds with path '/sitecore/templates/Animal template'",
                Severity = DiagnosticSeverity.Info,
                Locations =
                                    new[] {
                                            new DiagnosticResultLocation("Test0.cs", 11, 69)
                                        }
            };

            var expected = new DiagnosticResult
            {
                Id = "RainbowDataAnalyzerTemplateFieldIds",
                Message = "The field '{2ec9e41a-0d47-47ec-a0ac-2819edb60311}' is not on the template 'f5cfa142-fd92-4cdc-a6d5-c20020398418 (Animal template)' or on any of its base templates",
                Severity = DiagnosticSeverity.Error,
                Locations =
                            new[] {
                                    new DiagnosticResultLocation("Test0.cs", 2, 193)
                                }
            };

            string source = "[Glass.Mapper.Sc.Configuration.Attributes.SitecoreType(TemplateId = \"{f5cfa142-fd92-4cdc-a6d5-c20020398418}\")] class Animal { [Glass.Mapper.Sc.Configuration.Attributes.SitecoreField(FieldId = \"{2ec9e41a-0d47-47ec-a0ac-2819edb60311}\")] string CatHair { get;set; } }";
            
            this.ExecuteTest(source, expectedInfo, expected);
        }

        [TestMethod]
        public void CatsHaveFoodThatTheyLike()
        {
            var expectedInfo = new DiagnosticResult
            {
                Id = "RainbowDataAnalyzerIdToPath",
                Message = "The ID corresponds with path '/sitecore/templates/Cat template'",
                Severity = DiagnosticSeverity.Info,
                Locations =
                                                        new[] {
                                                                new DiagnosticResultLocation("Test0.cs", 11, 69)
                                                            }
            };

            var expectedInfo2 = new DiagnosticResult
            {
                Id = "RainbowDataAnalyzerIdToPath",
                Message = "The ID corresponds with path '/sitecore/templates/Animal template/Some section/Food that it likes'",
                Severity = DiagnosticSeverity.Info,
                Locations =
                                                            new[] {
                                                                    new DiagnosticResultLocation("Test0.cs", 11, 190)
                                                                }
            };

            string source = "[Glass.Mapper.Sc.Configuration.Attributes.SitecoreType(TemplateId = \"{05cfa142-fd92-4cdc-a6d5-c20020398418}\")] class Cat { [Glass.Mapper.Sc.Configuration.Attributes.SitecoreField(FieldId = \"{1ec9e41a-0d47-47ec-a0ac-2819edb60311}\")] string FoodThatItLikes { get;set; } }";
            
            this.ExecuteTest(source, expectedInfo, expectedInfo2);
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
