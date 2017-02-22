namespace RainbowDataAnalyzer.Rainbow
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Constants;
    using Microsoft.CodeAnalysis;

    public class Repository
    {
        /// <summary>
        /// The read all files lock
        /// </summary>
        private static readonly object readAllFilesLock = new object();

        /// <summary>
        /// Rainbow file hashes that can be used to keep reference to caches for already parsed files.
        /// </summary>
        private static readonly Dictionary<string, int> rainbowFileHashes = new Dictionary<string, int>();

        /// <summary>
        /// Cached versions of parsed .yml files.
        /// </summary>
        public static readonly Dictionary<string, RainbowFile> rainbowFiles = new Dictionary<string, RainbowFile>();


        /// <summary>
        /// Finds all templates for a field ID.
        /// </summary>
        /// <param name="files">The files.</param>
        /// <param name="fieldId">The field identifier.</param>
        /// <returns></returns>
        public static IEnumerable<RainbowFile> FindAllTemplatesForField(IEnumerable<AdditionalText> files, Guid fieldId)
        {
            List<RainbowFile> result = new List<RainbowFile>();
            lock (readAllFilesLock)
            {
                RainbowFile file;
                Evaluate(files, null, out file);

                var allRainbowFiles = rainbowFiles.Values;
                RainbowFile potentialTemplateFile = allRainbowFiles.FirstOrDefault(f => Guid.Equals(f.Id, fieldId));
                while (potentialTemplateFile != null)
                {
                    if (Guid.Equals(potentialTemplateFile.TemplateId, SitecoreConstants.SitecoreTemplateTemplateId))
                    {
                        break;
                    }

                    potentialTemplateFile = allRainbowFiles.FirstOrDefault(f => Guid.Equals(f.Id, potentialTemplateFile.ParentId));
                }

                if (potentialTemplateFile != null)
                {
                    result.Add(potentialTemplateFile);

                    var allTemplateFiles = allRainbowFiles.Where(f => Guid.Equals(f.TemplateId, SitecoreConstants.SitecoreTemplateTemplateId)).ToList();
                    result.AddRange(FindDerivedTemplates(allTemplateFiles, potentialTemplateFile, 200));
                }
            }

            return result.Distinct();
        }

        /// <summary>
        /// Finds derived templates recursively.
        /// </summary>
        /// <param name="allTemplateFiles">All template files.</param>
        /// <param name="templateFile">The template.</param>
        /// <param name="maxDepth">The maximum depth, to prevent stack overflow if there are circular references.</param>
        /// <returns></returns>
        private static IEnumerable<RainbowFile> FindDerivedTemplates(List<RainbowFile> allTemplateFiles, RainbowFile templateFile, int maxDepth)
        {
            if (maxDepth > 0)
            {
                foreach (RainbowFile derivedTemplate in allTemplateFiles.Where(t => t.BaseTemplates != null && t.BaseTemplates.Contains(templateFile.Id)))
                {
                    yield return derivedTemplate;

                    foreach (var derived in FindDerivedTemplates(allTemplateFiles, derivedTemplate, maxDepth - 1))
                    {
                        yield return derived;
                    }
                }
            }
        }

        /// <summary>
        /// Evaluates if any of the .yml files match the specified function.
        /// </summary>
        /// <param name="files">The .yml files.</param>
        /// <param name="evaluateFunction">The evaluation function.</param>
        /// <param name="matchingFile">The matching file.</param>
        /// <returns></returns>
        public static bool Evaluate(IEnumerable<AdditionalText> files, Func<RainbowFile, bool> evaluateFunction, out RainbowFile matchingFile)
        {
            matchingFile = null;
            foreach (AdditionalText text in files)
            {
                RainbowFile file;

                // This code will probably not run in parallel, but ensure that the dictionaries remain in sync just in case
                lock (rainbowFileHashes)
                {
                    if (rainbowFileHashes.ContainsKey(text.Path))
                    {
                        // We have already parsed this file, so check if our cache is up to date
                        if (rainbowFileHashes[text.Path] == text.GetHashCode())
                        {
                            // The cache is up to date, so use the cached version
                            file = rainbowFiles[text.Path];
                        }
                        else
                        {
                            // Update the cache
                            rainbowFileHashes[text.Path] = text.GetHashCode();
                            file = RainbowParserUtil.ParseRainbowFile(text);
                            rainbowFiles[text.Path] = file;
                        }
                    }
                    else
                    {
                        // Parse the file and add it to the cache
                        rainbowFileHashes.Add(text.Path, text.GetHashCode());
                        file = RainbowParserUtil.ParseRainbowFile(text);
                        rainbowFiles.Add(text.Path, file);
                    }
                }

                // Only stop evaluating if something matches
                if (evaluateFunction != null && evaluateFunction(file))
                {
                    matchingFile = file;
                    return true;
                }
            }

            return false;
        }
    }
}
