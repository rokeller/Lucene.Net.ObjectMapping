using System.Collections.Generic;
using System.Linq.Expressions;

namespace Lucene.Net.Mapping
{
    partial class MappedFieldResolver
    {
        /// <summary>
        /// A QuoteStrippingVisitor which can be used to find a mapped field.
        /// </summary>
        private sealed class FieldFinder : QuoteStrippingVisitor
        {
            /// <summary>
            /// The Stack of Expressions which are candidates for mapped fields.
            /// </summary>
            private readonly Stack<Expression> candidates = new Stack<Expression>();

            #region C'tors

            /// <summary>
            /// Initializes a new instance of FieldFinder.
            /// </summary>
            /// <param name="owner">
            /// The FieldNameResolver which owns this instance.
            /// </param>
            internal FieldFinder(MappedFieldResolver owner) : base(owner) { }

            #endregion

            /// <summary>
            /// Gets the MemberExpression which defines a field from the given Expression.
            /// </summary>
            /// <param name="expression">
            /// The Expression to find a field in.
            /// </param>
            /// <returns>
            /// An instance of MemberExpression if a field was found or null otherwise.
            /// </returns>
            internal MemberExpression GetField(Expression expression)
            {
                Visit(expression);

                if (candidates.Count > 1)
                {
                    return (MemberExpression)candidates.Pop();
                }

                return null;
            }

            #region Protected Methods

            /// <summary>
            /// Visits the children of the BinaryExpression.
            /// </summary>
            /// <param name="node">
            /// The expression to visit.
            /// </param>
            /// <returns>
            /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
            /// </returns>
            protected override Expression VisitBinary(BinaryExpression node)
            {
                candidates.Clear();

                return node;
            }

            /// <summary>
            /// Visits the children of the MemberExpression.
            /// </summary>
            /// <param name="node">
            /// The expression to visit.
            /// </param>
            /// <returns>
            /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
            /// </returns>
            protected override Expression VisitMember(MemberExpression node)
            {
                Expression container = Visit(node.Expression);
                if (candidates.Count > 0 && candidates.Peek() == container)
                {
                    candidates.Push(node);
                }

                return node;
            }

            /// <summary>
            /// Visits the children of the MethodCallExpression.
            /// </summary>
            /// <param name="node">
            /// The expression to visit.
            /// </param>
            /// <returns>
            /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
            /// </returns>
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                candidates.Clear();

                return node;
            }

            /// <summary>
            /// Visits the ParameterExpression.
            /// </summary>
            /// <param name="node">
            /// The expression to visit.
            /// </param>
            /// <returns>
            /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
            /// </returns>
            protected override Expression VisitParameter(ParameterExpression node)
            {
                candidates.Push(node);

                return node;
            }

            #endregion
        }
    }
}
