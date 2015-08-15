using System;

namespace Lucene.Net.Mapping
{
    /// <summary>
    /// Defines settings used to store objects in an index.
    /// </summary>
    public class StoreSettings
    {
        /// <summary>
        /// The default StoreSettings.
        /// </summary>
        /// <remarks>
        /// The default StoreSettings are as follows:
        /// - StoreSource: true (store the source object as '$source').
        /// </remarks>
        public static readonly StoreSettings Default = new StoreSettings()
        {
            StoreSource = true,
        };

        /// <summary>
        /// Gets or sets a flag which indicates whether or not to store the source object as '$source'.
        /// </summary>
        /// <remarks>
        /// Note: When the source is not stored in the index, the ObjectMapping library can no longer deserialize
        ///       documents into objects.
        /// </remarks>
        public bool StoreSource { get; set; }
    }
}
