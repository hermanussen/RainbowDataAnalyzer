namespace RainbowDataAnalyzer.Test.Rainbow
{
    using System;
    using System.Linq;
    using Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using RainbowDataAnalyzer.Rainbow;

    [TestClass]
    public class RainbowParserUtilTests
    {
        [TestMethod]
        public void ShouldParseId()
        {
            var result = RainbowParserUtil.ParseRainbowFile(new AnalyzerAdditionalFile(
                "/sitecore/somepath",
                @"---
ID: c715d20b-2e73-4046-9b87-749a11c27c5c"));
            Assert.AreEqual(Guid.Parse("c715d20b-2e73-4046-9b87-749a11c27c5c"), result.Id);

            // Try with quotes as well
            result = RainbowParserUtil.ParseRainbowFile(new AnalyzerAdditionalFile(
                "/sitecore/somepath",
                @"---
ID: ""c715d20b-2e73-4046-9b87-749a11c27c5c"""));
            Assert.AreEqual(Guid.Parse("c715d20b-2e73-4046-9b87-749a11c27c5c"), result.Id);
        }

        [TestMethod]
        public void ShouldParseParentId()
        {
            var result = RainbowParserUtil.ParseRainbowFile(new AnalyzerAdditionalFile(
                "/sitecore/somepath",
                @"---
ID: c715d20b-2e73-4046-9b87-749a11c27c5c
Parent: 8a4a4747-3278-437b-a12d-15d9ce6e1abf"));
            Assert.AreEqual(Guid.Parse("8a4a4747-3278-437b-a12d-15d9ce6e1abf"), result.ParentId);

            // Try with quotes as well
            result = RainbowParserUtil.ParseRainbowFile(new AnalyzerAdditionalFile(
                "/sitecore/somepath",
                @"---
ID: ""c715d20b-2e73-4046-9b87-749a11c27c5c""
Parent: ""8a4a4747-3278-437b-a12d-15d9ce6e1abf"""));
            Assert.AreEqual(Guid.Parse("8a4a4747-3278-437b-a12d-15d9ce6e1abf"), result.ParentId);
        }

        [TestMethod]
        public void ShouldParseTemplateId()
        {
            var result = RainbowParserUtil.ParseRainbowFile(new AnalyzerAdditionalFile(
                "/sitecore/somepath",
                @"---
ID: c715d20b-2e73-4046-9b87-749a11c27c5c
Parent: 8a4a4747-3278-437b-a12d-15d9ce6e1abf
Template: ab86861a-6030-46c5-b394-e8f99e8b87db"));
            Assert.AreEqual(Guid.Parse("ab86861a-6030-46c5-b394-e8f99e8b87db"), result.TemplateId);

            // Try with quotes as well
            result = RainbowParserUtil.ParseRainbowFile(new AnalyzerAdditionalFile(
                "/sitecore/somepath",
                @"---
ID: ""c715d20b-2e73-4046-9b87-749a11c27c5c""
Parent: ""8a4a4747-3278-437b-a12d-15d9ce6e1abf""
Template: ""ab86861a-6030-46c5-b394-e8f99e8b87db"""));
            Assert.AreEqual(Guid.Parse("ab86861a-6030-46c5-b394-e8f99e8b87db"), result.TemplateId);
        }

        [TestMethod]
        public void ShouldParsePath()
        {
            var result = RainbowParserUtil.ParseRainbowFile(new AnalyzerAdditionalFile(
                "/sitecore/somepath",
                @"---
ID: c715d20b-2e73-4046-9b87-749a11c27c5c
Parent: 8a4a4747-3278-437b-a12d-15d9ce6e1abf
Template: ab86861a-6030-46c5-b394-e8f99e8b87db
Path: /sitecore/templates/CommerceConnect/Products/Division"));
            Assert.AreEqual("/sitecore/templates/CommerceConnect/Products/Division", result.Path);
        }

        [TestMethod]
        public void ShouldParseBaseTemplates()
        {
            var result = RainbowParserUtil.ParseRainbowFile(new AnalyzerAdditionalFile(
                "/sitecore/somepath",
                @"---
ID: c715d20b-2e73-4046-9b87-749a11c27c5c
Parent: 8a4a4747-3278-437b-a12d-15d9ce6e1abf
Template: ab86861a-6030-46c5-b394-e8f99e8b87db
Path: /sitecore/templates/CommerceConnect/Products/Division
DB: master
SharedFields:
- ID: 12c33f3f-86c5-43a5-aeb4-5598cec45116
  Hint: __Base template
  Type: tree list
  Value: |
    {1930BBEB-7805-471A-A3BE-4858AC7CF696}
    {240334A5-CFE8-4450-BB85-253D620CBA02}
- ID: f7d48a55-2158-4f02-9356-756654404f73
  Hint: __Standard values
  Value: {FC617E68-8BAB-483A-98DA-6AC42DF86516}
Languages:
- Language: en
  Versions:
  - Version: 1
    Fields:
    - ID: 25bed78c-4957-4165-998a-ca1b52f67497
      Hint: __Created
      Value: 20130507T172955"));

            Assert.AreEqual("1930bbeb-7805-471a-a3be-4858ac7cf696|240334a5-cfe8-4450-bb85-253d620cba02", string.Join("|", result.BaseTemplates));
        }

        [TestMethod]
        public void ShouldParseBaseTemplate()
        {
            var result = RainbowParserUtil.ParseRainbowFile(new AnalyzerAdditionalFile(
                "/sitecore/somepath",
                @"---
ID: 29fe9b4e-bda0-420f-97ca-ce1421cd9cc8
Parent: 8a4a4747-3278-437b-a12d-15d9ce6e1abf
Template: ab86861a-6030-46c5-b394-e8f99e8b87db
Path: /sitecore/templates/CommerceConnect/Products/Identification Types
DB: master
SharedFields:
- ID: 12c33f3f-86c5-43a5-aeb4-5598cec45116
  Hint: __Base template
  Type: tree list
  Value: {1930BBEB-7805-471A-A3BE-4858AC7CF696}
- ID: f7d48a55-2158-4f02-9356-756654404f73
  Hint: __Standard values
  Value: {F14E8E3A-742F-46F7-9F10-A3151AC9C461}
Languages:
- Language: en
  Versions:
  - Version: 1
    Fields:
    - ID: 25bed78c-4957-4165-998a-ca1b52f67497
      Hint: __Created
      Value: 20130617T123737"));

            Assert.AreEqual("1930bbeb-7805-471a-a3be-4858ac7cf696", string.Join("|", result.BaseTemplates));
        }

        [TestMethod]
        public void ShouldNotParseBaseTemplate()
        {
            var result = RainbowParserUtil.ParseRainbowFile(new AnalyzerAdditionalFile(
                "/sitecore/somepath",
                @"---
ID: 29fe9b4e-bda0-420f-97ca-ce1421cd9cc8
Parent: 8a4a4747-3278-437b-a12d-15d9ce6e1abf
Template: bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb
Path: /sitecore/templates/CommerceConnect/Products/Identification Types
DB: master
SharedFields:
- ID: 12c33f3f-86c5-43a5-aeb4-5598cec45116
  Hint: __Base template
  Type: tree list
  Value: {1930BBEB-7805-471A-A3BE-4858AC7CF696}
- ID: f7d48a55-2158-4f02-9356-756654404f73
  Hint: __Standard values
  Value: {F14E8E3A-742F-46F7-9F10-A3151AC9C461}
Languages:
- Language: en
  Versions:
  - Version: 1
    Fields:
    - ID: 25bed78c-4957-4165-998a-ca1b52f67497
      Hint: __Created
      Value: 20130617T123737"));

            Assert.IsNull(result.BaseTemplates);

            result = RainbowParserUtil.ParseRainbowFile(new AnalyzerAdditionalFile(
                "/sitecore/somepath",
                @"---
ID: 29fe9b4e-bda0-420f-97ca-ce1421cd9cc8
Parent: 8a4a4747-3278-437b-a12d-15d9ce6e1abf
Template: ab86861a-6030-46c5-b394-e8f99e8b87db
Path: /sitecore/templates/CommerceConnect/Products/Identification Types
DB: master
SharedFields:
- ID: f7d48a55-2158-4f02-9356-756654404f73
  Hint: __Standard values
  Value: {F14E8E3A-742F-46F7-9F10-A3151AC9C461}
Languages:
- Language: en
  Versions:
  - Version: 1
    Fields:
    - ID: 25bed78c-4957-4165-998a-ca1b52f67497
      Hint: __Created
      Value: 20130617T123737"));

            Assert.IsFalse(result.BaseTemplates.Any());
        }
    }
}
