﻿using Lucene.Net.Documents;
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
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
            TypeNameHandling = TypeNameHandling.Auto,
        };

        #endregion

		#region Constructor(s)

		public JsonObjectMapper(Conventions conventions)
		{
			if (null == conventions)
				throw new ArgumentNullException("conventions");

			Conventions = conventions;
		}

		#endregion

		#region IObjectMapper Implementation

		public Conventions Conventions { get; set; }

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
        public QueryProvider GetQueryProvider(Searcher searcher)
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
        private void Add(Document doc, string prefix, JToken token)
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

				var stringFieldStore = (Conventions.ShouldStringFieldBeStored(prefix, value.Type)) ? 
					Field.Store.YES : Field.Store.NO;
				var stringFieldAnalyze = (Conventions.ShouldStringFieldBeAnalyzed(prefix, value.Type)) ?
					Field.Index.ANALYZED : Field.Index.NOT_ANALYZED;

                switch (value.Type)
                {
                    case JTokenType.Boolean:
                        doc.Add(new NumericField(prefix, Field.Store.NO, true).SetIntValue((bool)value.Value ? 1 : 0));
                        break;

                    case JTokenType.Date:
                        doc.Add(new NumericField(prefix, Field.Store.NO, true).SetLongValue(((DateTime)value.Value).Ticks));
                        break;

                    case JTokenType.Float:
                        if (value.Value is float)
                        {
                            doc.Add(new NumericField(prefix, Field.Store.NO, true).SetFloatValue((float)value.Value));
                        }
                        else
                        {
                            doc.Add(new NumericField(prefix, Field.Store.NO, true).SetDoubleValue(Convert.ToDouble(value.Value)));
                        }
                        break;

                    case JTokenType.Guid:
                        doc.Add(new Field(prefix, value.Value.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED));
                        break;

                    case JTokenType.Integer:
                        doc.Add(new NumericField(prefix, Field.Store.NO, true).SetLongValue(Convert.ToInt64(value.Value)));
                        break;

                    case JTokenType.Null:
                        break;

                    case JTokenType.String:
						doc.Add(new Field(prefix, value.Value.ToString(), stringFieldStore, stringFieldAnalyze));
                        break;

                    case JTokenType.TimeSpan:
                        doc.Add(new NumericField(prefix, Field.Store.NO, true).SetLongValue(((TimeSpan)value.Value).Ticks));
                        break;

                    case JTokenType.Uri:
						doc.Add(new Field(prefix, value.Value.ToString(), stringFieldStore, stringFieldAnalyze));
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
        private void AddProperties(Document doc, string prefix, JObject obj)
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
        private void AddArray(Document doc, string prefix, JArray array)
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
            public JsonObjectMapperQueryProvider(Searcher searcher) : base(searcher) { }

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
