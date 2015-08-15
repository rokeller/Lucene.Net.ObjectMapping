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
            StoreSettings = StoreSettings.Default,
        };

        /// <summary>
        /// Gets or stes the IObjectMapper to use.
        /// </summary>
        public IObjectMapper ObjectMapper { get; set; }

        /// <summary>
        /// Gets or sets a StoreSettings object which holds settings used to store objects in an index.
        /// </summary>
        public StoreSettings StoreSettings { get; set; }
    }
}
