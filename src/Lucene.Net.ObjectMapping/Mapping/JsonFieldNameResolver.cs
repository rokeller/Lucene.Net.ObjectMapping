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
        /// Gets the MappedField that describes the field mapped to the property of the given MemberExpression.
        /// </summary>
        /// <param name="member">
        /// The Expression to get the MappedField for.
        /// </param>
        /// <returns>
        /// A MappedField object which represents the mapped field, or null if the field is not mapped.
        /// </returns>
        protected override MappedField GetMappedField(MemberExpression member)
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
        /// Gets the field name from the given MemberExpression.
        /// </summary>
        /// <param name="member">
        /// The MemberExpression to get the field name from.
        /// </param>
        /// <returns>
        /// A string that represents the name of the field from the MemberExpression.
        /// </returns>
        private string GetFieldName(MemberExpression member)
        {
            StringBuilder sb = new StringBuilder(member.Member.Name);

            while (member.Expression is MemberExpression)
            {
                member = member.Expression as MemberExpression;

                sb.Insert(0, ".");
                sb.Insert(0, member.Member.Name);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the mapped FieldType for the field from the given MemberExpression.
        /// </summary>
        /// <param name="member">
        /// The MemberExpression to get the field type from.
        /// </param>
        /// <param name="memberType">
        /// If successful, holds the actual type of the member property or field being mapped.
        /// </param>
        /// <returns>
        /// A MappedField.FieldType that represents the type of the mapped field from the MemberExpression.
        /// </returns>
        private MappedField.FieldType GetFieldType(MemberExpression member, out Type memberType)
        {
            Type type = member.Type;
            Type enumerableMemberType = member.Type.GetEnumerableElementType();

            if (null != enumerableMemberType && type != enumerableMemberType)
            {
                type = enumerableMemberType;
            }

            memberType = type;

            if (type == typeof(long) ||
                type == typeof(int) || type == typeof(uint) ||
                type == typeof(short) || type == typeof(ushort) ||
                type == typeof(sbyte) || type == typeof(byte) ||
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
                    return (T)((object)expression.GetValue<Uri>().ToString());
                }
                else if (memberType == typeof(Guid))
                {
                    return (T)((object)expression.GetValue<Guid>().ToString());
                }
                else if (memberType == typeof(bool))
                {
                    return (T)((object)(expression.GetValue<bool>() ? 1 : 0));
                }

                throw new NotSupportedException(String.Format("The member type '{0}' is not supported.", memberType));
            }
        }

        #endregion
    }
}
