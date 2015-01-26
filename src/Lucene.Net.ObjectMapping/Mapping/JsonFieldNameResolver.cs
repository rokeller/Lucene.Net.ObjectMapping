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
            MappedField.FieldType type = GetFieldType(member);

            if (MappedField.FieldType.Unknown == type)
            {
                return null;
            }

            string name = GetFieldName(member);

            return new MappedField(name, type);
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
        /// <returns>
        /// A MappedField.FieldType that represents the type of the mapped field from the MemberExpression.
        /// </returns>
        private MappedField.FieldType GetFieldType(MemberExpression member)
        {
            Type type = member.Type;
            Type enumerableMemberType = member.Type.GetEnumerableElementType();

            if (null != enumerableMemberType && type != enumerableMemberType)
            {
                type = enumerableMemberType;
            }

            if (type == typeof(long) ||
                type == typeof(int) || type == typeof(uint) ||
                type == typeof(short) || type == typeof(ushort) ||
                type == typeof(sbyte) || type == typeof(byte))
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

            Debug.Fail("Unsupported member type: " + type);
            return MappedField.FieldType.Unknown;
        }

        #endregion
    }
}
