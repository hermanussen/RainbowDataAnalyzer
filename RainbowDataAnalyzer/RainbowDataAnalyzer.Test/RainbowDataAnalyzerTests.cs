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
    public class RainbowDataAnalyzerTests : DiagnosticVerifier
    {
        [TestMethod]
        public void ShouldNotGetMessages()
        {
            var test = @"";

            var yamlContents = new [] {
                "---\r\nID: \"0ec9e41a-0d47-47ec-a0ac-2819edb60311\"\r\nPath: /sitecore/existent"
            };

            this.VerifyCSharpDiagnostic(test, yamlContents);
        }
        
        [TestMethod]
        public void ShouldNotGetMessageAboutAssemblyGuid()
        {
            var test = "using System.Runtime.InteropServices;\r\n[assembly: Guid(\"1ec9e41a-0d47-47ec-a0ac-2819edb60311\")]";

            var yamlContents = new string[] {
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