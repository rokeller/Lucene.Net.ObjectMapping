using System;

namespace Lucene.Net.Mapping
{
    /// <summary>
    /// Defines settings for mapping objects to Lucene.Net Documents.
    /// </summary>
    public sealed class MappingSettings
    {
        /// <summary>
        /// The default MappingSettings.
        /// </summary>
        public static readonly MappingSettings Default = new MappingSettings()
        {
            ObjectMapper = new JsonObjectMapper(),
        };

        /// <summary>
        /// Gets or stes the IObjectMapper to use.
        /// </summary>
        public IObjectMapper ObjectMapper { get; set; }
    }
}
