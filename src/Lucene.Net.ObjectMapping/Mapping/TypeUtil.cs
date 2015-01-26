using System;
using System.Collections.Generic;

namespace Lucene.Net.Mapping
{
    /// <summary>
    /// Utilities for Type objects.
    /// </summary>
    public static class TypeUtil
    {
        /// <summary>
        /// Gets the element type of the element type used by the specified enumerable seqType.
        /// </summary>
        /// <param name="sequenceType">
        /// The enumerable Type for which the element type is to be found.
        /// </param>
        /// <returns>
        /// </returns>
        public static Type GetEnumerableElementType(this Type sequenceType)
        {
            Type enumType = FindIEnumerable(sequenceType);

            if (enumType == null)
            {
                return sequenceType;
            }

            return enumType.GetGenericArguments()[0];
        }

        /// <summary>
        /// Finds the generic IEnumerable implemented by the given seqType.
        /// </summary>
        /// <param name="sequenceType">
        /// The Type that should implement a generic IEnumerable.
        /// </param>
        /// <returns>
        /// The IEnumerable type implemented by the given seqType, or null if no such type was found.
        /// </returns>
        private static Type FindIEnumerable(this Type sequenceType)
        {
            if (sequenceType == null || sequenceType == typeof(string))
            {
                return null;
            }

            if (sequenceType.IsArray)
            {
                return typeof(IEnumerable<>).MakeGenericType(sequenceType.GetElementType());
            }
            else if (sequenceType.IsGenericType)
            {
                foreach (Type arg in sequenceType.GetGenericArguments())
                {
                    Type enumType = typeof(IEnumerable<>).MakeGenericType(arg);

                    if (enumType.IsAssignableFrom(sequenceType))
                    {
                        return enumType;
                    }
                }
            }

            Type[] interfaces = sequenceType.GetInterfaces();

            if (interfaces != null && interfaces.Length > 0)
            {
                foreach (Type iface in interfaces)
                {
                    Type enumType = FindIEnumerable(iface);

                    if (enumType != null)
                    {
                        return enumType;
                    }
                }
            }

            if (sequenceType.BaseType != null && sequenceType.BaseType != typeof(object))
            {
                return FindIEnumerable(sequenceType.BaseType);
            }

            return null;
        }
    }
}
