using System;

namespace Lucene.Net
{
    /// <summary>
    /// Utilities for this library.
    /// </summary>
    internal sealed class Utils
    {
        /// <summary>
        /// Gets the type name (for serialization) for the given type.
        /// </summary>
        /// <param name="type">
        /// The Type to get the name for.
        /// </param>
        /// <returns>
        /// A String that can be used to uniquely identify the type.
        /// </returns>
        /// <remarks>
        /// This creates a type name in the format "My.Name.Space.MyClass, MyAssembly", i.e. the type and assembly name
        /// without any extra information such as public key, assembly version number or culture.
        /// </remarks>
        internal static string GetTypeName(Type type)
        {
            return String.Format("{0}, {1}", type.FullName, type.Assembly.GetName().Name);
        }
    }
}
