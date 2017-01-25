namespace RainbowDataAnalyzer
{
    using System;

    /// <summary>
    /// Keeps the info for a .yml file that is parsed.
    /// </summary>
    public class RainbowFile
    {
        /// <summary>
        /// Gets or sets the Sitecore ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the Sitecore path.
        /// </summary>
        public string Path { get; set; }
    }
}
