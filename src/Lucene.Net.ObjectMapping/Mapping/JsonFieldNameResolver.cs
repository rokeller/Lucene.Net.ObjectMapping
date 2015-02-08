using Lucene.Net.Linq;
using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;

namespace Lucene.Net.Mapping
{
    /// <summary>
    /// Implements a MappedFieldResolver for the JsonObjectMapper.
    /// </summary>
    internal sealed class JsonFieldNameResolver : MappedFieldResolver
    {
        #region Protected Methods

        /// <summary>
        /// Gets the MappedField that describes the field mapped to the property of the given Expression.
        /// </summary>
        /// <param name="member">
        /// The Expression to get the MappedField for.
        /// </param>
        /// <returns>
        /// A MappedField object which represents the mapped field, or null if the field is not mapped.
        /// </returns>
        protected override MappedField GetMappedField(Expression member)
        {
            Type memberType;
            MappedField.FieldType type = GetFieldType(member, out memberType);

            if (MappedField.FieldType.Unknown == type)
            {
                return null;
            }

            string name = GetFieldName(member);

            return new JsonMappedField(memberType, name, type);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the field name from the given Expression.
        /// </summary>
        /// <param name="expression">
        /// The Expression to get the field name from.
        /// </param>
        /// <returns>
        /// A string that represents the name of the field from the Expression.
        /// </returns>
        private string GetFieldName(Expression expression)
        {
            StringBuilder sb = new StringBuilder();

            AddFieldName(expression, sb);

            // The string builder now starts with a period, which we need to remove.
            sb.Remove(0, 1);

            return sb.ToString();
        }

        /// <summary>
        /// Adds the field name from the given Expression to the specified StringBuilder.
        /// </summary>
        /// <param name="expression">
        /// The Expression to add the field name for.
        /// </param>
        /// <param name="sb">
        /// The StringBuilder to add the field name to.
        /// </param>
        private void AddFieldName(Expression expression, StringBuilder sb)
        {
            MemberExpression member = expression as MemberExpression;
            MethodCallExpression call = expression as MethodCallExpression;
            ParameterExpression param = expression as ParameterExpression;

            if (null != member)
            {
                AddFieldName(member, sb);
            }
            else if (null != call)
            {
                AddFieldName(call, sb);
            }
            else if (null != param)
            {
                // Nothing todo, since we found the param.
            }
            else
            {
                throw new NotSupportedException(String.Format("Cannot find the field for the expression: {0}.", expression));
            }
        }

        /// <summary>
        /// Adds the field name from the given MemberExpression to the specified StringBuilder.
        /// </summary>
        /// <param name="expression">
        /// The MemberExpression to add the field name for.
        /// </param>
        /// <param name="sb">
        /// The StringBuilder to add the field name to.
        /// </param>
        private void AddFieldName(MemberExpression expression, StringBuilder sb)
        {
            AddFieldName(expression.Expression, sb);
            sb.Append('.');
            sb.Append(expression.Member.Name);
        }

        /// <summary>
        /// Adds the field name from the given MethodCallExpression to the specified StringBuilder.
        /// </summary>
        /// <param name="expression">
        /// The MethodCallExpression to add the field name for.
        /// </param>
        /// <param name="sb">
        /// The StringBuilder to add the field name to.
        /// </param>
        private void AddFieldName(MethodCallExpression expression, StringBuilder sb)
        {
            if (IsDictionaryGetItem(expression))
            {
                AddFieldName(expression.Object, sb);
                sb.Append('.');
                sb.Append(expression.Arguments[0].GetValue<string>());
            }
        }

        /// <summary>
        /// Gets the mapped FieldType for the field from the given Expression.
        /// </summary>
        /// <param name="expression">
        /// The Expression to get the field type from.
        /// </param>
        /// <param name="memberType">
        /// If successful, holds the actual type of the member property or field being mapped.
        /// </param>
        /// <returns>
        /// A MappedField.FieldType that represents the type of the mapped field from the Expression.
        /// </returns>
        private MappedField.FieldType GetFieldType(Expression expression, out Type memberType)
        {
            Type type = expression.Type;
            Type enumerableMemberType = expression.Type.GetEnumerableElementType();

            if (null != enumerableMemberType && type != enumerableMemberType)
            {
                type = enumerableMemberType;
            }

            memberType = type;

            if (type == typeof(long) ||
                type == typeof(int) || type == typeof(uint) ||
                type == typeof(short) || type == typeof(ushort) ||
                type == typeof(sbyte) || type == typeof(byte) ||
                type == typeof(DateTime) ||
                type.IsEnum)
            {
                return MappedField.FieldType.Long;
            }
            else if (type == typeof(float))
            {
                return MappedField.FieldType.Float;
            }
            else if (type == typeof(double) || type == typeof(decimal))
            {
                return MappedField.FieldType.Double;
            }
            else if (type == typeof(string) || type == typeof(Uri) || type == typeof(Guid))
            {
                return MappedField.FieldType.String;
            }
            else if (type == typeof(bool))
            {
                return MappedField.FieldType.Int;
            }

            Debug.Fail("Unsupported member type: " + type);
            return MappedField.FieldType.Unknown;
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Represents a MappedField for the JsonObjectMapper.
        /// </summary>
        private sealed class JsonMappedField : MappedField
        {
            /// <summary>
            /// The Type of the member tracked with this field.
            /// </summary>
            private readonly Type memberType;

            /// <summary>
            /// Initializes a new instance of JsonMappedField.
            /// </summary>
            /// <param name="memberType">
            /// The type of the member that exposes this field.
            /// </param>
            /// <param name="name">
            /// The name of the mapped field.
            /// </param>
            /// <param name="type">
            /// The FieldType of the mapped field.
            /// </param>
            public JsonMappedField(Type memberType, string name, FieldType type)
                : base(name, type)
            {
                if (null == memberType)
                {
                    throw new ArgumentNullException("memberType");
                }

                this.memberType = memberType;
            }

            /// <summary>
            /// Gets the value of type T from the given expression.
            /// </summary>
            /// <typeparam name="T">
            /// The type of the value to get.
            /// </typeparam>
            /// <param name="expression">
            /// The expression to get the value from.
            /// </param>
            /// <returns>
            /// An object of type T.
            /// </returns>
            public override T GetValueFromExpression<T>(Expression expression)
            {
                if (memberType == typeof(long) ||
                    memberType == typeof(int) || memberType == typeof(uint) ||
                    memberType == typeof(short) || memberType == typeof(ushort) ||
                    memberType == typeof(sbyte) || memberType == typeof(byte) ||
                    memberType.IsEnum)
                {
                    return (T)((object)expression.GetValue<long>());
                }
                else if (memberType == typeof(float))
                {
                    return (T)((object)expression.GetValue<float>());
                }
                else if (memberType == typeof(double) || memberType == typeof(decimal))
                {
                    return (T)((object)expression.GetValue<double>());
                }
                else if (memberType == typeof(string))
                {
                    return (T)((object)expression.GetValue<string>());
                }
                else if (memberType == typeof(Uri))
                {
                    Uri uri = expression.GetValue<Uri>();

                    return (T)((object)uri.ToString());
                }
                else if (memberType == typeof(Guid))
                {
                    Guid guid = expression.GetValue<Guid>();

                    return (T)((object)guid.ToString());
                }
                else if (memberType == typeof(bool))
                {
                    int val = expression.GetValue<bool>() ? 1 : 0;

                    return (T)((object)val);
                }
                else if (memberType == typeof(DateTime))
                {
                    long ticks = expression.GetValue<DateTime>().Ticks;

                    return (T)((object)ticks);
                }

                throw new NotSupportedException(String.Format("The member type '{0}' is not supported.", memberType));
            }
        }

        #endregion
    }
}
