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

        /// <summary>
        /// Gets or sets the Sitecore template ID.
        /// </summary>
        public Guid TemplateId { get; set; }

        public string ItemName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.Path))
                {
                    return null;
                }

                if (!this.Path.TrimEnd('/').Contains("/"))
                {
                    return this.Path;
                }

                return this.Path.Substring(this.Path.TrimEnd('/').LastIndexOf('/') + 1);
            }
        }
    }
}
