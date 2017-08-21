using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Queries;
using System;
using System.Diagnostics;

namespace Lucene.Net.Search
{
    /// <summary>
    /// Helper class for object mapping related to search in Lucene.Net.
    /// </summary>
    public static class ObjectMapping
    {
        /// <summary>
        /// Gets a Filter to restrict results to documents that are mapped from objects of the given type.
        /// </summary>
        /// <param name="type">
        /// The Type to get the filter for.
        /// </param>
        /// <returns>
        /// An instance of Filter.
        /// </returns>
        public static Filter GetTypeFilter(Type type)
        {
            TermsFilter filter = new TermsFilter(
                                    new Term(Documents.ObjectMappingExtensions.FieldActualType, Utils.GetTypeName(type)),
                                    new Term(Documents.ObjectMappingExtensions.FieldStaticType, Utils.GetTypeName(type)));
            return filter;
        }

        /// <summary>
        /// Gets a Filter to restrict results to documents that are mapped from objects of the given type.
        /// </summary>
        /// <typeparam name="T">
        /// The Type to get the filter for.
        /// </typeparam>
        /// <returns>
        /// An instance of Filter.
        /// </returns>
        public static Filter GetTypeFilter<T>()
        {
            return GetTypeFilter(typeof(T));
        }

        /// <summary>
        /// Gets a Filter to restrict results to documents that are mapped from objects of the given type.
        /// </summary>
        /// <param name="type">
        /// The Type to get the filter for.
        /// </param>
        /// <param name="kind">
        /// The kind of type to restrict the filter on.
        /// </param>
        /// <returns>
        /// An instance of Filter.
        /// </returns>
        public static Filter GetTypeFilter(Type type, DocumentObjectTypeKind kind)
        {
            switch (kind)
            {
                case DocumentObjectTypeKind.Actual:
                    return new TermsFilter(new Term(Documents.ObjectMappingExtensions.FieldActualType, Utils.GetTypeName(type)));

                case DocumentObjectTypeKind.Static:
                    return new TermsFilter(new Term(Documents.ObjectMappingExtensions.FieldStaticType, Utils.GetTypeName(type)));

                default:
                    Debug.Fail("Unsupported DocumentObjectType: " + kind);
                    throw new NotSupportedException(String.Format("The DocumentObjectType '{0}' is not supported.", kind));
            }
        }

        /// <summary>
        /// Gets a Filter to restrict results to documents that are mapped from objects of the given type.
        /// </summary>
        /// <typeparam name="T">
        /// The Type to get the filter for.
        /// </typeparam>
        /// <param name="kind">
        /// The kind of type to restrict the filter on.
        /// </param>
        /// <returns>
        /// An instance of Filter.
        /// </returns>
        public static Filter GetTypeFilter<T>(DocumentObjectTypeKind kind)
        {
            return GetTypeFilter(typeof(T), kind);
        }
    }
}
