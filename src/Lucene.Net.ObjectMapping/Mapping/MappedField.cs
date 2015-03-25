using Lucene.Net.Linq;
using System;
using System.Linq.Expressions;

namespace Lucene.Net.Mapping
{
    /// <summary>
    /// Represents a mapped field.
    /// </summary>
    public class MappedField
    {
        /// <summary>
        /// Initializes a new instance of MappedField.
        /// </summary>
        /// <param name="name">
        /// The name of the mapped field.
        /// </param>
        /// <param name="type">
        /// The FieldType of the mapped field.
        /// </param>
        public MappedField(string name, FieldType type)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }
            else if (!Enum.IsDefined(typeof(FieldType), type))
            {
                throw new ArgumentException("The type parameter must be one of the supported FieldType values.");
            }
            else if (FieldType.Unknown == type)
            {
                throw new ArgumentException("The type parameter must not be set to 'Unknown'.");
            }

            Name = name;
            Type = type;
        }

        /// <summary>
        /// Gets or sets the name of the mapped field.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the FieldType of the mapped field.
        /// </summary>
        public FieldType Type { get; private set; }

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
        public virtual T GetValueFromExpression<T>(Expression expression)
        {
            return expression.GetValue<T>();
        }

        /// <summary>
        /// Defines the types a mapped field can have.
        /// </summary>
        public enum FieldType
        {
            /// <summary>
            /// The type is unknown.
            /// </summary>
            Unknown,

            /// <summary>
            /// The field is a float field.
            /// </summary>
            Float,

            /// <summary>
            /// The field is a double field.
            /// </summary>
            Double,

            /// <summary>
            /// The field is a short field.
            /// </summary>
            Short,

            /// <summary>
            /// The field is an int field.
            /// </summary>
            Int,

            /// <summary>
            /// The field is a long field.
            /// </summary>
            Long,

            /// <summary>
            /// The field is a string field.
            /// </summary>
            String,
        }
    }
}
