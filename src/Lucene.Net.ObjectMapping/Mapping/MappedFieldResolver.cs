using Lucene.Net.Search;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Lucene.Net.Mapping
{
    /// <summary>
    /// Defines the MappedFieldResolver base class to help resolve mapped fields in various situations.
    /// </summary>
    public abstract partial class MappedFieldResolver
    {
        #region Public Methods

        /// <summary>
        /// Gets the SortField for the given Expression.
        /// </summary>
        /// <param name="expression">
        /// The Expression to get the SortField for.
        /// </param>
        /// <param name="sortDescending">
        /// True if sorting should be descending, false otherwise.
        /// </param>
        /// <returns>
        /// An instance of SortField.
        /// </returns>
        public SortField GetSortField(Expression expression, bool sortDescending)
        {
            SortFieldVisitor visitor = new SortFieldVisitor(this, sortDescending);

            return visitor.GetSortField(expression);
        }

        /// <summary>
        /// Gets the Query for the given Expression.
        /// </summary>
        /// <param name="expression">
        /// The Expression to get the Query for.
        /// </param>
        /// <returns>
        /// An instance of Query.
        /// </returns>
        public Query GetQuery(Expression expression)
        {
            QueryVisitor visitor = new QueryVisitor(this);

            return visitor.GetQuery(expression);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Gets the MappedField that describes the field mapped to the property of the given MemberExpression.
        /// </summary>
        /// <param name="member">
        /// The Expression to get the MappedField for.
        /// </param>
        /// <returns>
        /// A MappedField object which represents the mapped field, or null if the field is not mapped.
        /// </returns>
        protected abstract MappedField GetMappedField(MemberExpression member);

        #endregion

        #region Helper Classes

        /// <summary>
        /// Base class for an ExpressionVisitor which strips Quotes from expressions.
        /// </summary>
        private abstract class QuoteStrippingVisitor : ExpressionVisitor
        {
            /// <summary>
            /// Gets or set the FieldNameResolver which owns this instance.
            /// </summary>
            protected MappedFieldResolver Owner { get; private set; }

            /// <summary>
            /// Initializes a new instance of QuoteStrippingVisitor.
            /// </summary>
            /// <param name="owner">
            /// The FieldNameResolver which owns this instance.
            /// </param>
            protected QuoteStrippingVisitor(MappedFieldResolver owner)
            {
                Debug.Assert(null != owner, "The owner must not be null.");

                Owner = owner;
            }

            /// <summary>
            /// Visits the children of the UnaryExpression.
            /// </summary>
            /// <param name="node">
            /// The expression to visit.
            /// </param>
            /// <returns>
            /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
            /// </returns>
            protected override Expression VisitUnary(UnaryExpression node)
            {
                switch (node.NodeType)
                {
                    case ExpressionType.Quote:
                        return Visit(StripQuotes(node));

                    default:
                        return base.VisitUnary(node);
                }
            }

            /// <summary>
            /// Strips the quotes off of the given Expression.
            /// </summary>
            /// <param name="node">
            /// The Expression to strip the quotes off from.
            /// </param>
            /// <returns>
            /// An Expression for which the Quotes were stripped off.
            /// </returns>
            private static Expression StripQuotes(Expression node)
            {
                while (node.NodeType == ExpressionType.Quote)
                {
                    node = ((UnaryExpression)node).Operand;
                }

                return node;
            }
        }

        #endregion
    }
}
