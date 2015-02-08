using System;
using System.Linq.Expressions;

namespace Lucene.Net.Linq
{
    /// <summary>
    /// Utilities for Expression objects.
    /// </summary>
    internal static class ExpressionUtil
    {
        /// <summary>
        /// Tries to get the value of type T from the given Expression.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the expected result.
        /// </typeparam>
        /// <param name="expression">
        /// The Expression to get the value from.
        /// </param>
        /// <returns>
        /// An instance of T that represents the value of the given Expression.
        /// </returns>
        public static T GetValue<T>(this Expression expression)
        {
            if (null == expression)
            {
                throw new ArgumentNullException("expression");
            }

            T val;

            if (!expression.TryGetConstant(out val))
            {
                expression = Expression.Convert(expression, typeof(T));
                Expression<Func<T>> lambda = Expression.Lambda<Func<T>>(expression);
                Func<T> func = lambda.Compile();
                val = func();
            }

            return val;
        }

        /// <summary>
        /// Tries to get the constant value of type T from the given Expression.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the expected result.
        /// </typeparam>
        /// <param name="expression">
        /// The Expression to get the value from.
        /// </param>
        /// <param name="value">
        /// If successful, holds the value of the contant in the expression.
        /// </param>
        /// <returns>
        /// True if successful, false otherwise.
        /// </returns>
        public static bool TryGetConstant<T>(this Expression expression, out T value)
        {
            if (null == expression)
            {
                throw new ArgumentNullException("expression");
            }

            ConstantExpression constant = expression as ConstantExpression;

            if (null == constant ||
                (constant.Type != typeof(T) && !typeof(T).IsAssignableFrom(constant.Type)))
            {
                value = default(T);

                return false;
            }

            value = (T)constant.Value;

            return true;
        }
    }
}
