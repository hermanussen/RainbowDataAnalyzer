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
 ```
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

## Installation of the Visual Studio extension only

Install RainbowDataAnalyzer from the extensions and updates window.

## How it works

This is a Roslyn analyzer, which means that it plugs into the compiler API's to check the code syntax and/or semantics. This particular analyzer currently checks all string literal expressions.
- If there are no `AdditionalFiles` specified in the project file, nothing will be validated
- If the string starts with `/sitecore/`, we will assume that it is a sitecore path
- If the string can be parsed as a guid, we will assume that it is a sitecore ID
- In the AssemblyInfo.cs file, you may find a guid in a string as well; this is ignored
- If the string or ID refers to a field, we will check if it can be found in the serialized data as well
- Especially if you use [precompiled views](http://kamsar.net/index.php/2016/09/Precompiled-Views-with-Sitecore-8-2/), the @Html.Sitecore().Field(...) syntax is also validated.

Furthermore, if you follow the following rules, the analyzer will also check if the fields that are being used are on the right template.
- The field needs to be on an item which is assigned to a variable
- You need to call an extension method on that variable (which can have any implementation you want; it can also be empty) with the following signature: `public static void MustDeriveFrom([NotNull] this Item item, [NotNull] string templatePathOrId)`
- The call to `MustDeriveFrom(...)` and the accessing of a field must be within the same method (which may also be a Razor View)

If you want to change the severity of the checks (e.g.: make something a `warning` instead of an `error`), you can do this for individual projects in Visual Studio:
- Expand the project's references
- Right click `Analyzers` and click `Open Active Ruleset`
- Now change the settings for the `RainbowDataAnalyzer` rules

## Future plans

I'd like to add support for the following features in the future. If you have any more suggestions, please add an issue.
  - Make it less strict, as there still appear to be some false positives
  - Implement intellisense on paths and field names (some work on this in branch `intellisense-paths-and-fields`, but currently stopped because [Visual Studio does not do completion inside string literals](http://stackoverflow.com/questions/31096360/how-to-auto-complete-string-literal-in-visual-studio))
  - Implement code fix for switching between ID and Path
  - Make field on template checking more context aware for Razor views (e.g.: check context model)
  - Add support for Glass Mapper, Synthesis and Fortis model checking
  
## Release notes

1.2.0
  - Field names and IDs can now also be checked to be on the correct template

1.1.0
  - Added support for checking fields

1.0.0
  - Analyzer implemented that checks for Sitecore IDs and paths