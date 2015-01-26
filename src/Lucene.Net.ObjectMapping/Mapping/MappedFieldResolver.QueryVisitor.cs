using Lucene.Net.Index;
using Lucene.Net.Linq;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Lucene.Net.Mapping
{
    partial class MappedFieldResolver
    {
        /// <summary>
        /// A QuoteStrippingVisitor which can be used to get a Lucene.Net Query for an Expression.
        /// </summary>
        private sealed class QueryVisitor : QuoteStrippingVisitor
        {
            /// <summary>
            /// Holds the stack of Query objects that are being parsed.
            /// </summary>
            private readonly Stack<Query> queryStack = new Stack<Query>();

            /// <summary>
            /// Initializes a new instance of QueryVisitor.
            /// </summary>
            /// <param name="owner">
            /// The FieldNameResolver which owns this instance.
            /// </param>
            internal QueryVisitor(MappedFieldResolver owner) : base(owner) { }

            /// <summary>
            /// Gets the Query for the given Expression.
            /// </summary>
            /// <param name="expression">
            /// The Expression to get the Query for.
            /// </param>
            /// <returns>
            /// An instance of Query.
            /// </returns>
            internal Query GetQuery(Expression expression)
            {
                Visit(expression);
                Debug.Assert(queryStack.Count == 1, "There must be exactly one element on the query stack.");

                return queryStack.Pop();
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
                if (node.Method.DeclaringType == typeof(QueryOperators))
                {
                    if (node.Method.Name == "InRange")
                    {
                        Expression targetField = node.Arguments[0];

                        if (!(targetField is MemberExpression))
                        {
                            throw new NotSupportedException("The expression is not supported for IsInRange: " + targetField);
                        }

                        MappedField field = Owner.GetMappedField(targetField as MemberExpression);

                        if (null == field)
                        {
                            throw new InvalidOperationException("The expression refers to a field which is not mapped: " + targetField);
                        }

                        Query query = MakeNumericRangeQuery(
                            field,
                            node.Arguments[1],
                            node.Arguments[2],
                            node.Arguments[3],
                            node.Arguments[4]);
                        queryStack.Push(query);

                        return node;
                    }
                    else if (node.Method.Name == "MatchesTerm")
                    {
                        Expression targetField = node.Arguments[0];

                        if (!(targetField is MemberExpression))
                        {
                            throw new NotSupportedException("The expression is not supported for MatchesTerm: " + targetField);
                        }

                        MappedField field = Owner.GetMappedField(targetField as MemberExpression);

                        if (null == field)
                        {
                            throw new InvalidOperationException("The expression refers to a field which is not mapped: " + targetField);
                        }

                        string term = node.Arguments[1].GetValue<string>();
                        Query query = new TermQuery(new Term(field.Name, term));
                        queryStack.Push(query);

                        return node;
                    }
                }

                return base.VisitMethodCall(node);
            }

            /// <summary>
            /// Creates a new NumericRangeQuery for the given parameters.
            /// </summary>
            /// <param name="field">
            /// The MappedField to create the query for.
            /// </param>
            /// <param name="min">
            /// The Expression for the minimum value.
            /// </param>
            /// <param name="max">
            /// The Expression for the maximum value.
            /// </param>
            /// <param name="minInclusive">
            /// The Expression to determine whether or not the minimum value is inclusive.
            /// </param>
            /// <param name="maxInclusive">
            /// The Expression to determine whether or not the maximum value is inclusive.
            /// </param>
            /// <returns>
            /// A Query which represents the NumericRangeQuery for the given parameters.
            /// </returns>
            private Query MakeNumericRangeQuery(MappedField field,
                                                Expression min,
                                                Expression max,
                                                Expression minInclusive,
                                                Expression maxInclusive)
            {
                Debug.Assert(null != field, "The mapped field must not be null.");
                Debug.Assert(null != min, "The min expression must not be null.");
                Debug.Assert(null != max, "The max expression must not be null.");
                Debug.Assert(null != minInclusive, "The minInclusive expression must not be null.");
                Debug.Assert(null != maxInclusive, "The maxInclusive expression must not be null.");

                switch (field.Type)
                {
                    case MappedField.FieldType.Float:
                        return NumericRangeQuery.NewFloatRange(field.Name,
                                                               min.GetValue<float?>(),
                                                               max.GetValue<float?>(),
                                                               minInclusive.GetValue<bool>(),
                                                               maxInclusive.GetValue<bool>());

                    case MappedField.FieldType.Double:
                        return NumericRangeQuery.NewDoubleRange(field.Name,
                                                                min.GetValue<double?>(),
                                                                max.GetValue<double?>(),
                                                                minInclusive.GetValue<bool>(),
                                                                maxInclusive.GetValue<bool>());

                    case MappedField.FieldType.Short:
                        return NumericRangeQuery.NewIntRange(field.Name,
                                                             min.GetValue<short?>(),
                                                             max.GetValue<short?>(),
                                                             minInclusive.GetValue<bool>(),
                                                             maxInclusive.GetValue<bool>());

                    case MappedField.FieldType.Int:
                        return NumericRangeQuery.NewIntRange(field.Name,
                                                             min.GetValue<int?>(),
                                                             max.GetValue<int?>(),
                                                             minInclusive.GetValue<bool>(),
                                                             maxInclusive.GetValue<bool>());

                    case MappedField.FieldType.Long:
                        return NumericRangeQuery.NewLongRange(field.Name,
                                                              min.GetValue<long?>(),
                                                              max.GetValue<long?>(),
                                                              minInclusive.GetValue<bool>(),
                                                              maxInclusive.GetValue<bool>());

                    default:
                        throw new NotSupportedException(String.Format("Numeric Range Query is not supported for type '{0}'.", field.Type));
                }
            }
        }
    }
}
