using Lucene.Net.Index;
using Lucene.Net.Linq;
using Lucene.Net.Search;
using Lucene.Net.Util;
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

            #region C'tors

            /// <summary>
            /// Initializes a new instance of QueryVisitor.
            /// </summary>
            /// <param name="owner">
            /// The FieldNameResolver which owns this instance.
            /// </param>
            internal QueryVisitor(MappedFieldResolver owner) : base(owner) { }

            #endregion

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

            #region Protected Methods

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
                if (node.Method.Name == "Equals" && 1 == node.Arguments.Count)
                {
                    return VisitBinary(Expression.MakeBinary(ExpressionType.Equal, node.Object, node.Arguments[0]));
                }
                else if (node.Method.DeclaringType == typeof(QueryOperators))
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
                Expression value = null;
                MappedField mappedField = null;
                BooleanQuery boolQuery;

                switch (node.NodeType)
                {
                    case ExpressionType.Equal:
                        mappedField = GetFieldAndConstant(node, out value);
                        Debug.Assert(null != mappedField, "The mapped field must not be null.");
                        Debug.Assert(null != value, "The value expression must not be null.");

                        queryStack.Push(MakeTermQuery(mappedField, value));

                        return node;

                    case ExpressionType.NotEqual:
                        VisitBinary(Expression.MakeBinary(ExpressionType.Equal, node.Left, node.Right));
                        Debug.Assert(queryStack.Count >= 1, "The query stack must not be empty.");

                        boolQuery = new BooleanQuery();
                        boolQuery.Add(new MatchAllDocsQuery(), Occur.MUST);
                        boolQuery.Add(queryStack.Pop(), Occur.MUST_NOT);
                        queryStack.Push(boolQuery);

                        return node;

                    case ExpressionType.AndAlso:
                        Visit(node.Left);
                        Visit(node.Right);
                        Debug.Assert(queryStack.Count >= 2, "The query stack must have at least two queries.");

                        boolQuery = new BooleanQuery();
                        boolQuery.Add(queryStack.Pop(), Occur.MUST);
                        boolQuery.Add(queryStack.Pop(), Occur.MUST);
                        queryStack.Push(boolQuery);

                        return node;

                    case ExpressionType.OrElse:
                        Visit(node.Left);
                        Visit(node.Right);
                        Debug.Assert(queryStack.Count >= 2, "The query stack must have at least two queries.");

                        boolQuery = new BooleanQuery();
                        boolQuery.Add(queryStack.Pop(), Occur.SHOULD);
                        boolQuery.Add(queryStack.Pop(), Occur.SHOULD);
                        queryStack.Push(boolQuery);

                        return node;
                }

                throw new NotSupportedException(String.Format("Unsupported binary Expression: {0}", node));
            }

            /// <summary>
            /// Visits the ConstantExpression.
            /// </summary>
            /// <param name="node">
            /// The expression to visit.
            /// </param>
            /// <returns>
            /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
            /// </returns>
            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (node.Type == typeof(bool))
                {
                    if ((bool)node.Value)
                    {
                        queryStack.Push(new MatchAllDocsQuery());
                    }
                    else
                    {
                        BooleanQuery query = new BooleanQuery();
                        query.Add(new MatchAllDocsQuery(), Occur.MUST_NOT);

                        queryStack.Push(query);
                    }

                    return node;
                }

                return base.VisitConstant(node);
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
                if (node.Type != typeof(bool))
                {
                    throw new NotSupportedException(String.Format("The expression is not supported: {0}.", node));
                }

                MappedField field = Owner.GetMappedField(node);
                Query query = MakeTermQuery(field, Expression.Constant(true));

                queryStack.Push(query);

                return node;
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
                BooleanQuery boolQuery;

                switch (node.NodeType)
                {
                    case ExpressionType.Not:
                        if (node.Operand.NodeType == ExpressionType.MemberAccess)
                        {
                            // We're accessing a boolean member, and we want to match those where the field is false.
                            MappedField field = Owner.GetMappedField((MemberExpression)node.Operand);
                            Query query = MakeTermQuery(field, Expression.Constant(false));

                            queryStack.Push(query);

                            return node;
                        }

                        Visit(node.Operand);
                        Debug.Assert(queryStack.Count >= 1, "The query stack must not be empty.");

                        boolQuery = new BooleanQuery();
                        boolQuery.Add(new MatchAllDocsQuery(), Occur.MUST);
                        boolQuery.Add(queryStack.Pop(), Occur.MUST_NOT);
                        queryStack.Push(boolQuery);

                        return node;
                }

                return base.VisitUnary(node);
            }

            #endregion

            #region Private Methods

            /// <summary>
            /// Tries to get a mapped field and a constant from the given BinaryExpression.
            /// </summary>
            /// <param name="expression">
            /// The BinaryExpression to get the field and constant for.
            /// </param>
            /// <param name="value">
            /// If successful, holds the Expression which defines the constant.
            /// </param>
            /// <returns>
            /// An instance of MappedField.
            /// </returns>
            private MappedField GetFieldAndConstant(BinaryExpression expression, out Expression value)
            {
                Expression firstMember;
                Expression secondMember;

                firstMember = new FieldFinder(Owner).GetField(expression.Left);
                secondMember = new FieldFinder(Owner).GetField(expression.Right);

                if (null != firstMember && null != secondMember)
                {
                    if (IsConstant(secondMember))
                    {
                        value = expression.Right;
                        return Owner.GetMappedField(firstMember);
                    }
                    else if (IsConstant(firstMember))
                    {
                        value = expression.Left;
                        return Owner.GetMappedField(secondMember);
                    }
                    else
                    {
                        throw new NotSupportedException(String.Format(
                            "Queries comparing two members are not supported: {0}.", expression));
                    }
                }
                else if (null != firstMember)
                {
                    value = expression.Right;
                    return Owner.GetMappedField(firstMember);
                }
                else if (null != secondMember)
                {
                    value = expression.Left;
                    return Owner.GetMappedField(secondMember);
                }

                throw new NotSupportedException(String.Format(
                    "The query is not supported: {0}.", expression));
            }

            /// <summary>
            /// Checks if the given Expression is constant, i.e. it is a member of a constant.
            /// </summary>
            /// <param name="expression">
            /// The Expression to check.
            /// </param>
            /// <returns>
            /// True if the Expression is constant, false otherwise.
            /// </returns>
            private static bool IsConstant(Expression expression)
            {
                MemberExpression member = expression as MemberExpression;

                while (null != member && member.Expression is MemberExpression)
                {
                    member = (MemberExpression)member.Expression;
                }

                return null != member && member.Expression is ConstantExpression;
            }

            /// <summary>
            /// Makes a TermQuery for the given MappedField and expression.
            /// </summary>
            /// <param name="field">
            /// The MappedField to create the query for.
            /// </param>
            /// <param name="term">
            /// The constant to match in the query.
            /// </param>
            /// <returns>
            /// An instance of TermQuery.
            /// </returns>
            private static TermQuery MakeTermQuery(MappedField field, Expression term)
            {
                Debug.Assert(null != field, "The mapped field must not be null.");
                Debug.Assert(null != term, "The term must not be null.");

                Term tm;

                switch (field.Type)
                {
                    case MappedField.FieldType.Float:
                        {
                            var bytes = new BytesRef(NumericUtils.BUF_SIZE_INT64);
                            long l = NumericUtils.DoubleToSortableInt64(field.GetValueFromExpression<float>(term));
                            NumericUtils.Int64ToPrefixCoded(l, 0, bytes);
                            tm = new Term(field.Name, bytes);
                            //queryTerm = NumericUtils.FloatToPrefixCoded(field.GetValueFromExpression<float>(term));
                            break;
                        }

                    case MappedField.FieldType.Double:
                        {
                            var bytes = new BytesRef(NumericUtils.BUF_SIZE_INT64);
                            long l = NumericUtils.DoubleToSortableInt64(field.GetValueFromExpression<double>(term));
                            NumericUtils.Int64ToPrefixCoded(l, 0, bytes);
                            tm = new Term(field.Name, bytes);
                            //queryTerm = NumericUtils.DoubleToPrefixCoded(field.GetValueFromExpression<double>(term));
                            break;
                        }
                    case MappedField.FieldType.Short:
                        {
                            var bytes = new BytesRef(NumericUtils.BUF_SIZE_INT32);
                            NumericUtils.Int32ToPrefixCoded(field.GetValueFromExpression<int>(term), 0, bytes);
                            tm = new Term(field.Name, bytes);
                            break;
                        }
                    case MappedField.FieldType.Int:
                        {
                            var bytes = new BytesRef(NumericUtils.BUF_SIZE_INT32);
                            NumericUtils.Int32ToPrefixCoded(field.GetValueFromExpression<int>(term), 0, bytes);
                            tm = new Term(field.Name, bytes);
                            //queryTerm = NumericUtils.IntToPrefixCoded(field.GetValueFromExpression<int>(term));
                            break;
                        }

                    case MappedField.FieldType.Long:
                        {
                            var bytes = new BytesRef(NumericUtils.BUF_SIZE_INT64);
                            NumericUtils.Int64ToPrefixCoded(field.GetValueFromExpression<long>(term), 0, bytes);
                            tm = new Term(field.Name, bytes);
                            //queryTerm = NumericUtils.LongToPrefixCoded(field.GetValueFromExpression<long>(term));
                            break;
                        }
                    case MappedField.FieldType.String:
                        {
                            tm = new Term(field.Name, field.GetValueFromExpression<string>(term));
                            break;
                        }
                    default:
                        throw new InvalidOperationException(String.Format(
                            "Cannot make a TermQuery for field '{0}' of type {1}.",
                            field.Name, field.Type));
                }

                return new TermQuery(tm );
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
            private static Query MakeNumericRangeQuery(MappedField field,
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
                        return NumericRangeQuery.NewDoubleRange(field.Name,
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
                        return NumericRangeQuery.NewInt32Range(field.Name,
                                                             min.GetValue<short?>(),
                                                             max.GetValue<short?>(),
                                                             minInclusive.GetValue<bool>(),
                                                             maxInclusive.GetValue<bool>());

                    case MappedField.FieldType.Int:
                        return NumericRangeQuery.NewInt32Range(field.Name,
                                                             min.GetValue<int?>(),
                                                             max.GetValue<int?>(),
                                                             minInclusive.GetValue<bool>(),
                                                             maxInclusive.GetValue<bool>());

                    case MappedField.FieldType.Long:
                        return NumericRangeQuery.NewInt64Range(field.Name,
                                                              min.GetValue<long?>(),
                                                              max.GetValue<long?>(),
                                                              minInclusive.GetValue<bool>(),
                                                              maxInclusive.GetValue<bool>());

                    default:
                        throw new NotSupportedException(String.Format("Numeric Range Query is not supported for type '{0}'.", field.Type));
                }
            }

            #endregion
        }
    }
}
