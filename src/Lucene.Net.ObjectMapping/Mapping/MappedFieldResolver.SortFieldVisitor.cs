using Lucene.Net.Search;
using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Lucene.Net.Mapping
{
    partial class MappedFieldResolver
    {
        /// <summary>
        /// A QuoteStrippingVisitor which can be used to get a Lucene.Net SortField for an Expression.
        /// </summary>
        private sealed class SortFieldVisitor : QuoteStrippingVisitor
        {
            #region Fields

            /// <summary>
            /// A flag which indicates whether or not the SortField should be set to sort descending.
            /// </summary>
            private readonly bool sortDescending;

            /// <summary>
            /// The SortField that results from this visitor.
            /// </summary>
            private SortField sortField;

            #endregion

            #region C'tors

            /// <summary>
            /// Initializes a new instance of SortFieldVisitor.
            /// </summary>
            /// <param name="owner">
            /// The MappedFieldResolver which owns this instance.
            /// </param>
            /// <param name="sortDescending">
            /// A flag which indicates whether or not the SortField should be set to sort descending.
            /// </param>
            internal SortFieldVisitor(MappedFieldResolver owner, bool sortDescending)
                : base(owner)
            {
                this.sortDescending = sortDescending;
            }

            #endregion

            /// <summary>
            /// Gets the SortField for the given Expression.
            /// </summary>
            /// <param name="expression">
            /// The Expression to get the sort field for.
            /// </param>
            /// <returns>
            /// An instance of SortField which can be used to sort on the field from the given Expression.
            /// </returns>
            public SortField GetSortField(Expression expression)
            {
                Visit(expression);
                Debug.Assert(null != sortField, "The resulting sort field must not be null.");

                return sortField;
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
                MappedField mappedField = Owner.GetMappedField(node);

                if (null == mappedField)
                {
                    throw new InvalidOperationException("The expression refers to a field which is not mapped: " + node);
                }

                int fieldType;

                switch (mappedField.Type)
                {
                    case MappedField.FieldType.Float:
                        fieldType = SortField.FLOAT;
                        break;

                    case MappedField.FieldType.Double:
                        fieldType = SortField.DOUBLE;
                        break;

                    case MappedField.FieldType.Short:
                        fieldType = SortField.SHORT;
                        break;

                    case MappedField.FieldType.Int:
                        fieldType = SortField.INT;
                        break;

                    case MappedField.FieldType.Long:
                        fieldType = SortField.LONG;
                        break;

                    case MappedField.FieldType.String:
                        fieldType = SortField.STRING;
                        break;

                    default:
                        Debug.Fail("The FieldType is not supported: " + mappedField.Type);
                        throw new NotSupportedException("The FieldType is not supported: " + mappedField.Type);
                }

                sortField = new SortField(mappedField.Name, fieldType, sortDescending);

                return node;
            }
        }
    }
}
