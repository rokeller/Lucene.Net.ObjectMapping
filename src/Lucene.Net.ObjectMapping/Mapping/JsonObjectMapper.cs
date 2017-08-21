using Lucene.Net.Documents;
using Lucene.Net.Linq;
using Lucene.Net.Search;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters;

namespace Lucene.Net.Mapping
{
    /// <summary>
    /// Implements the IObjectMapper interface using a JSON serialization to identify all the properties and nested
    /// objects to map/add to a Lucene.Net Document. This mapper doesn't store any of the fields on the document but
    /// instead just adds them to be indexed as appropriate.
    /// </summary>
    public sealed class JsonObjectMapper : IObjectMapper
    {
        #region Fields

        /// <summary>
        /// The JsonSerializer to use.
        /// </summary>
        private static readonly JsonSerializer serializer = new JsonSerializer()
        {
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            TypeNameHandling = TypeNameHandling.Auto,
        };

        #endregion

        #region IObjectMapper Implementation

        /// <summary>
        /// Adds the given source object to the specified Document.
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of the object to add.
        /// </typeparam>
        /// <param name="source">
        /// The source object to add.
        /// </param>
        /// <param name="doc">
        /// The Document to add the object to.
        /// </param>
        public void AddToDocument<TObject>(TObject source, Document doc)
        {
            JToken token = JToken.FromObject(source, serializer);
            Add(doc, null, token);
        }

        /// <summary>
        /// Gets the MappedFieldResolver used by this instance.
        /// </summary>
        /// <returns>
        /// A MappedFieldResolver.
        /// </returns>
        public MappedFieldResolver GetMappedFieldResolver()
        {
            return new JsonFieldNameResolver();
        }

        /// <summary>
        /// Gets the QueryProvider used by this instance.
        /// </summary>
        /// <param name="searcher">
        /// The Searcher to use for the QueryProvider.
        /// </param>
        /// <returns>
        /// A QueryProvider.
        /// </returns>
        public QueryProvider GetQueryProvider(IndexSearcher searcher)
        {
            return new JsonObjectMapperQueryProvider(searcher);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds the given JToken to the specified Document.
        /// </summary>
        /// <param name="doc">
        /// The Document to add to.
        /// </param>
        /// <param name="prefix">
        /// The prefix to use for field names.
        /// </param>
        /// <param name="token">
        /// The JToken to add.
        /// </param>
        private static void Add(Document doc, string prefix, JToken token)
        {
            if (token is JObject)
            {
                AddProperties(doc, prefix, token as JObject);
            }
            else if (token is JArray)
            {
                AddArray(doc, prefix, token as JArray);
            }
            else if (token is JValue)
            {
                JValue value = token as JValue;
                IConvertible convertible = value as IConvertible;

                switch (value.Type)
                {
                    case JTokenType.Boolean:
                        var boolFieldType = new FieldType(Int32Field.TYPE_NOT_STORED);
                        boolFieldType.IsIndexed = true;
                        doc.Add(new Int32Field(prefix, (bool)value.Value ? 1 : 0, boolFieldType));
                        break;

                    case JTokenType.Date:
                        var dateFieldType = new FieldType(Int64Field.TYPE_NOT_STORED);
                        dateFieldType.IsIndexed = true;
                        doc.Add(new Int64Field(prefix, ((DateTime)value.Value).Ticks, dateFieldType));
                        break;

                    case JTokenType.Float:
                        var floatFieldType = new FieldType(DoubleField.TYPE_NOT_STORED);
                        floatFieldType.IsIndexed = true;

                        if (value.Value is float)
                        {
                            doc.Add(new DoubleField(prefix, (float)value.Value, floatFieldType));
                        }
                        else
                        {
                            doc.Add(new DoubleField(prefix, Convert.ToDouble(value.Value), floatFieldType)); 
                        }
                        break;

                    case JTokenType.Guid:
                        doc.Add(new StringField(prefix, value.Value.ToString(), Field.Store.NO));
                        break;

                    case JTokenType.Integer:
                        var intFieldType = new FieldType(Int64Field.TYPE_NOT_STORED);
                        intFieldType.IsIndexed = true;
                        doc.Add(new Int64Field(prefix, Convert.ToInt64(value.Value), intFieldType));
                        break;

                    case JTokenType.Null:
                        break;

                    case JTokenType.String:
                        doc.Add(new TextField(prefix, value.Value.ToString(),Field.Store.NO));
                        break;

                    case JTokenType.TimeSpan:
                        var tsFieldType = new FieldType(Int64Field.TYPE_NOT_STORED);
                        tsFieldType.IsIndexed = true;
                        doc.Add(new Int64Field(prefix, ((TimeSpan)value.Value).Ticks, tsFieldType));
                        break;

                    case JTokenType.Uri:
                        doc.Add(new TextField(prefix, value.Value.ToString(), Field.Store.NO));
                        break;

                    default:
                        Debug.Fail("Unsupported JValue type: " + value.Type);
                        break;
                }
            }
            else
            {
                Debug.Fail("Unsupported JToken: " + token);
            }
        }

        /// <summary>
        /// Adds the properties of the given JObject to the specified Document.
        /// </summary>
        /// <param name="doc">
        /// The Document to add the properties to.
        /// </param>
        /// <param name="prefix">
        /// The prefix to use for field names.
        /// </param>
        /// <param name="obj">
        /// The JObject to add.
        /// </param>
        private static void AddProperties(Document doc, string prefix, JObject obj)
        {
            foreach (JProperty property in obj.Properties())
            {
                Add(doc, MakePrefix(prefix, property.Name), property.Value);
            }
        }

        /// <summary>
        /// Adds the elements of the given JArray to the specified Document.
        /// </summary>
        /// <param name="doc">
        /// The Document to add the elements to.
        /// </param>
        /// <param name="prefix">
        /// The prefix to use for field names.
        /// </param>
        /// <param name="array">
        /// The JArray to add.
        /// </param>
        private static void AddArray(Document doc, string prefix, JArray array)
        {
            for (int i = 0; i < array.Count; i++)
            {
                Add(doc, prefix, array[i]);
            }
        }

        /// <summary>
        /// Makes a prefix for field names.
        /// </summary>
        /// <typeparam name="TAdd">
        /// The Type of the last part to add to the prefix.
        /// </typeparam>
        /// <param name="prefix">
        /// The existing prefix to extend.
        /// </param>
        /// <param name="add">
        /// The part to add to the prefix.
        /// </param>
        /// <returns>
        /// A string that can be used as prefix for field names.
        /// </returns>
        private static string MakePrefix<TAdd>(string prefix, TAdd add)
        {
            if (!String.IsNullOrEmpty(prefix))
            {
                return String.Format("{0}.{1}", prefix, add);
            }
            else
            {
                return add.ToString();
            }
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Implements a QueryProvider for the JsonObjectMapper.
        /// </summary>
        private sealed class JsonObjectMapperQueryProvider : QueryProvider
        {
            /// <summary>
            /// Initializes a new instance of JsonObjectMapperQueryProvider.
            /// </summary>
            /// <param name="searcher">
            /// The Searcher to use.
            /// </param>
            public JsonObjectMapperQueryProvider(IndexSearcher searcher) : base(searcher) { }

            /// <summary>
            /// Gets the FieldNameResolver to use with this instance.
            /// </summary>
            public override MappedFieldResolver FieldNameResolver
            {
                get { return new JsonFieldNameResolver(); }
            }
        }

        #endregion
    }
}
