# Rainbow data analyzer

This project contains the source code for the Visual Studio extension and NuGet package of a Roslyn analyzer that checks Sitecore ID's and paths with serialized data. The Rainbow data format that is used by Unicorn is supported here.

## Installation prerequisites
- Tested with Visual Studio 2015 using C# 6
- Ensure that the Unicorn .yml files are part of the project where you want the validation to work (more info about this below)

## Include .yml files in your project

Roslyn can only access files that are included in the project. They don't have to be visible, though and you can use wildards. So it's really not that big of a deal.

However, try to limit the amount of files that are included, as it may make things very slow if your project needs to load thousands of files. It would be good practice anyway to limit the files to those that are needed by your project; in line with Helix principles.

Steps to include files:
  - Unload the project in Visual Studio
  - Edit the project file
  - Add the following lines somewhere in the project node and change the path
 ```xml
   <ItemGroup>
    <AdditionalFiles Include="..\Sitecore.Data\Unicorn\templates\**\*.yml">
      <Visible>false</Visible>
    </AdditionalFiles>
  </ItemGroup>
 ```
  - You can actually add multiple entries like this if you want

## Installation to integrate this into your build

Install the NuGet package from the package manager or the package manager console:
```
Install-Package RainbowDataAnalyzer
```

## Installation of the Visual Studio extension

Install RainbowDataAnalyzer from the extensions and updates window.

## How it works

This is a Roslyn analyzer, which means that it plugs into the compiler API's to check the code syntax and/or semantics. This particular analyzer currently checks all string literal expressions.
- If there are no `AdditionalFiles` specified in the project file, nothing will be validated
- If the string starts with `/sitecore/`, we will assume that it is a sitecore path
- If the string can be parsed as a guid, we will assume that it is a sitecore ID
- In the AssemblyInfo.cs file, you may find a guid in a string as well; this is ignored

## Future plans

I'd like to add support for the following features in the future. If you have any more suggestions, please add an issue.
  - Improve unit tests (there are already a few accurate ones)
  - Make it less strict, as there still appear to be some false positives
  - Make the severity configurable in Visual Studio
  - Validate field names and field ID's (without looking at the template)
  - Find a way to track objects and verify template compatibility of field names and field id's