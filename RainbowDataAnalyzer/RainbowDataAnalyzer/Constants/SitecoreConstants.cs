namespace RainbowDataAnalyzer.Constants
{
    using System;

    internal static class SitecoreConstants
    {
        /// <summary>
        /// The identifier syntax names that point to things that require a field name or ID.
        /// </summary>
        internal static readonly string[] IdentifierSyntaxNames = { "Item", "Fields", "Field", "BeginField" };

        /// <summary>
        /// The ID for the template of a template field
        /// </summary>
        internal static readonly Guid SitecoreTemplateFieldId = Guid.Parse("{455A3E98-A627-4B40-8035-E683A0331AC7}");

        /// <summary>
        /// The ID for the template of a template itself
        /// </summary>
        internal static readonly Guid SitecoreTemplateTemplateId = Guid.Parse("{AB86861A-6030-46C5-B394-E8F99E8B87DB}");

    }
}
