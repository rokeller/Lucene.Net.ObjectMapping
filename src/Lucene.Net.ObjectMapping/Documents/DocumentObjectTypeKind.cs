using System;

namespace Lucene.Net.Documents
{
    /// <summary>
    /// Defines the kind of types used when storing type information of a mapped object to a Document.
    /// </summary>
    public enum DocumentObjectTypeKind
    {
        /// <summary>
        /// The actual / dynamic type of the object.
        /// </summary>
        Actual,

        /// <summary>
        /// The static (compile-time) type of the object.
        /// </summary>
        Static,
    }
}
