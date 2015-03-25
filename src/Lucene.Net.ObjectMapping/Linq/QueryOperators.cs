using System;

namespace Lucene.Net.Linq
{
    /// <summary>
    /// Extensions for Query operators.
    /// </summary>
    public static class QueryOperators
    {
        #region Number Range Queries

        /// <summary>
        /// Reduces the query results to items within the given range.
        /// </summary>
        /// <param name="value">
        /// The value to check.
        /// </param>
        /// <param name="min">
        /// The minimum value or null if there is no minimum value to check against.
        /// </param>
        /// <param name="max">
        /// The maximum value or null if there is no minimum value to check against.
        /// </param>
        /// <param name="minInclusive">
        /// True if the minimum value is inclusive, false otherwise.
        /// </param>
        /// <param name="maxInclusive">
        /// True if the maximum value is inclusive, false otherwise.
        /// </param>
        /// <returns>
        /// True if it is in range, false otherwise.
        /// </returns>
        public static bool InRange(this long value, long? min, long? max, bool minInclusive, bool maxInclusive)
        {
            throw new NotSupportedException("Calling InRange directly is not supported.");
        }

        /// <summary>
        /// Reduces the query results to items within the given range.
        /// </summary>
        /// <param name="value">
        /// The value to check.
        /// </param>
        /// <param name="min">
        /// The minimum value or null if there is no minimum value to check against.
        /// </param>
        /// <param name="max">
        /// The maximum value or null if there is no minimum value to check against.
        /// </param>
        /// <param name="minInclusive">
        /// True if the minimum value is inclusive, false otherwise.
        /// </param>
        /// <param name="maxInclusive">
        /// True if the maximum value is inclusive, false otherwise.
        /// </param>
        /// <returns>
        /// True if it is in range, false otherwise.
        /// </returns>
        public static bool InRange(this int value, int? min, int? max, bool minInclusive, bool maxInclusive)
        {
            throw new NotSupportedException("Calling InRange directly is not supported.");
        }

        /// <summary>
        /// Reduces the query results to items within the given range.
        /// </summary>
        /// <param name="value">
        /// The value to check.
        /// </param>
        /// <param name="min">
        /// The minimum value or null if there is no minimum value to check against.
        /// </param>
        /// <param name="max">
        /// The maximum value or null if there is no minimum value to check against.
        /// </param>
        /// <param name="minInclusive">
        /// True if the minimum value is inclusive, false otherwise.
        /// </param>
        /// <param name="maxInclusive">
        /// True if the maximum value is inclusive, false otherwise.
        /// </param>
        /// <returns>
        /// True if it is in range, false otherwise.
        /// </returns>
        public static bool InRange(this uint value, uint? min, uint? max, bool minInclusive, bool maxInclusive)
        {
            throw new NotSupportedException("Calling InRange directly is not supported.");
        }

        /// <summary>
        /// Reduces the query results to items within the given range.
        /// </summary>
        /// <param name="value">
        /// The value to check.
        /// </param>
        /// <param name="min">
        /// The minimum value or null if there is no minimum value to check against.
        /// </param>
        /// <param name="max">
        /// The maximum value or null if there is no minimum value to check against.
        /// </param>
        /// <param name="minInclusive">
        /// True if the minimum value is inclusive, false otherwise.
        /// </param>
        /// <param name="maxInclusive">
        /// True if the maximum value is inclusive, false otherwise.
        /// </param>
        /// <returns>
        /// True if it is in range, false otherwise.
        /// </returns>
        public static bool InRange(this short value, short? min, short? max, bool minInclusive, bool maxInclusive)
        {
            throw new NotSupportedException("Calling InRange directly is not supported.");
        }

        /// <summary>
        /// Reduces the query results to items within the given range.
        /// </summary>
        /// <param name="value">
        /// The value to check.
        /// </param>
        /// <param name="min">
        /// The minimum value or null if there is no minimum value to check against.
        /// </param>
        /// <param name="max">
        /// The maximum value or null if there is no minimum value to check against.
        /// </param>
        /// <param name="minInclusive">
        /// True if the minimum value is inclusive, false otherwise.
        /// </param>
        /// <param name="maxInclusive">
        /// True if the maximum value is inclusive, false otherwise.
        /// </param>
        /// <returns>
        /// True if it is in range, false otherwise.
        /// </returns>
        public static bool InRange(this ushort value, ushort? min, ushort? max, bool minInclusive, bool maxInclusive)
        {
            throw new NotSupportedException("Calling InRange directly is not supported.");
        }

        /// <summary>
        /// Reduces the query results to items within the given range.
        /// </summary>
        /// <param name="value">
        /// The value to check.
        /// </param>
        /// <param name="min">
        /// The minimum value or null if there is no minimum value to check against.
        /// </param>
        /// <param name="max">
        /// The maximum value or null if there is no minimum value to check against.
        /// </param>
        /// <param name="minInclusive">
        /// True if the minimum value is inclusive, false otherwise.
        /// </param>
        /// <param name="maxInclusive">
        /// True if the maximum value is inclusive, false otherwise.
        /// </param>
        /// <returns>
        /// True if it is in range, false otherwise.
        /// </returns>
        public static bool InRange(this sbyte value, sbyte? min, sbyte? max, bool minInclusive, bool maxInclusive)
        {
            throw new NotSupportedException("Calling InRange directly is not supported.");
        }

        /// <summary>
        /// Reduces the query results to items within the given range.
        /// </summary>
        /// <param name="value">
        /// The value to check.
        /// </param>
        /// <param name="min">
        /// The minimum value or null if there is no minimum value to check against.
        /// </param>
        /// <param name="max">
        /// The maximum value or null if there is no minimum value to check against.
        /// </param>
        /// <param name="minInclusive">
        /// True if the minimum value is inclusive, false otherwise.
        /// </param>
        /// <param name="maxInclusive">
        /// True if the maximum value is inclusive, false otherwise.
        /// </param>
        /// <returns>
        /// True if it is in range, false otherwise.
        /// </returns>
        public static bool InRange(this byte value, byte? min, byte? max, bool minInclusive, bool maxInclusive)
        {
            throw new NotSupportedException("Calling InRange directly is not supported.");
        }

        /// <summary>
        /// Reduces the query results to items within the given range.
        /// </summary>
        /// <param name="value">
        /// The value to check.
        /// </param>
        /// <param name="min">
        /// The minimum value or null if there is no minimum value to check against.
        /// </param>
        /// <param name="max">
        /// The maximum value or null if there is no minimum value to check against.
        /// </param>
        /// <param name="minInclusive">
        /// True if the minimum value is inclusive, false otherwise.
        /// </param>
        /// <param name="maxInclusive">
        /// True if the maximum value is inclusive, false otherwise.
        /// </param>
        /// <returns>
        /// True if it is in range, false otherwise.
        /// </returns>
        public static bool InRange(this float value, float? min, float? max, bool minInclusive, bool maxInclusive)
        {
            throw new NotSupportedException("Calling InRange directly is not supported.");
        }

        /// <summary>
        /// Reduces the query results to items within the given range.
        /// </summary>
        /// <param name="value">
        /// The value to check.
        /// </param>
        /// <param name="min">
        /// The minimum value or null if there is no minimum value to check against.
        /// </param>
        /// <param name="max">
        /// The maximum value or null if there is no minimum value to check against.
        /// </param>
        /// <param name="minInclusive">
        /// True if the minimum value is inclusive, false otherwise.
        /// </param>
        /// <param name="maxInclusive">
        /// True if the maximum value is inclusive, false otherwise.
        /// </param>
        /// <returns>
        /// True if it is in range, false otherwise.
        /// </returns>
        public static bool InRange(this double value, double? min, double? max, bool minInclusive, bool maxInclusive)
        {
            throw new NotSupportedException("Calling InRange directly is not supported.");
        }

        /// <summary>
        /// Reduces the query results to items within the given range.
        /// </summary>
        /// <param name="value">
        /// The value to check.
        /// </param>
        /// <param name="min">
        /// The minimum value or null if there is no minimum value to check against.
        /// </param>
        /// <param name="max">
        /// The maximum value or null if there is no minimum value to check against.
        /// </param>
        /// <param name="minInclusive">
        /// True if the minimum value is inclusive, false otherwise.
        /// </param>
        /// <param name="maxInclusive">
        /// True if the maximum value is inclusive, false otherwise.
        /// </param>
        /// <returns>
        /// True if it is in range, false otherwise.
        /// </returns>
        public static bool InRange(this decimal value, decimal? min, decimal? max, bool minInclusive, bool maxInclusive)
        {
            throw new NotSupportedException("Calling InRange directly is not supported.");
        }

        #endregion

        #region Term Queries

        /// <summary>
        /// Reduces the query results to items which match the given term.
        /// </summary>
        /// <typeparam name="TField">
        /// The type of the property to match the term on.
        /// </typeparam>
        /// <param name="value">
        /// The property value to match the term on.
        /// </param>
        /// <param name="term">
        /// The term to match. This must already be a valid term, e.g. turned into a lower-case string if the field's
        /// analyzer does that too.
        /// </param>
        /// <returns>
        /// True if the term matches, false otherwise.
        /// </returns>
        public static bool MatchesTerm<TField>(this TField value, string term)
        {
            throw new NotSupportedException("Calling MatchesTerm directly is not supported.");
        }

        #endregion
    }
}
