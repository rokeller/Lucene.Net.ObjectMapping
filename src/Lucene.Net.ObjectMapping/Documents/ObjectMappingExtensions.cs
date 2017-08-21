using Lucene.Net.Mapping;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization.Formatters;

namespace Lucene.Net.Documents
{
    /// <summary>
    /// Extension class to help mapping objects to Documents and vice versa.
    /// </summary>
    public static class ObjectMappingExtensions
    {
        #region Consts

        /// <summary>
        /// The name of the field which holds the object's actual type.
        /// </summary>
        public static readonly string FieldActualType = "$actualType";

        /// <summary>
        /// The name of the field which holds the object's static type.
        /// </summary>
        public static readonly string FieldStaticType = "$staticType";

        /// <summary>
        /// The name of the field which holds the JSON-serialized source of the object.
        /// </summary>
        public static readonly string FieldSource = "$source";

        /// <summary>
        /// The name of the field which holds the timestamp when the document was created.
        /// </summary>
        public static readonly string FieldTimestamp = "$timestamp";

        #endregion

        #region Fields

        /// <summary>
        /// The JsonSerializerSettings for serialization and deserialization of objects to/from JSON.
        /// </summary>
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            TypeNameHandling = TypeNameHandling.Auto,
        };

        #endregion

        #region Public Methods

        /// <summary>
        /// Maps the source object to a Lucene.Net Document.
        /// </summary>
        /// <typeparam name="TSource">
        /// The Type of the source object to map.
        /// </typeparam>
        /// <param name="source">
        /// The source object to map.
        /// </param>
        /// <returns>
        /// An instance of Document that represents the mapped object.
        /// </returns>
        public static Document ToDocument<TSource>(this TSource source)
        {
            return ToDocument<TSource>(source, MappingSettings.Default);
        }

        /// <summary>
        /// Maps the source object to a Lucene.Net Document.
        /// </summary>
        /// <typeparam name="TSource">
        /// The Type of the source object to map.
        /// </typeparam>
        /// <param name="source">
        /// The source object to map.
        /// </param>
        /// <param name="mappingSettings">
        /// The MappingSettings to use.
        /// </param>
        /// <returns>
        /// An instance of Document that represents the mapped object.
        /// </returns>
        public static Document ToDocument<TSource>(this TSource source, MappingSettings mappingSettings)
        {
            if (null == mappingSettings)
            {
                throw new ArgumentNullException("mappingSettings");
            }
            else if (null == mappingSettings.ObjectMapper)
            {
                throw new ArgumentNullException("mappingSettings.ObjectMapper");
            }
            else if (null == mappingSettings.StoreSettings)
            {
                throw new ArgumentNullException("mappingSettings.StoreSettings");
            }

            Document doc = new Document();
            string json = JsonConvert.SerializeObject(source, typeof(TSource), settings);

            FieldType ft = new FieldType(StringField.TYPE_STORED);
            ft.OmitNorms = false;

            doc.Add(new Field(FieldActualType, Utils.GetTypeName(source.GetType()), ft)); 
            doc.Add(new Field(FieldStaticType, Utils.GetTypeName(typeof(TSource)), ft));

            FieldType storedAndNotIndexFT = new FieldType(StringField.TYPE_STORED);
            storedAndNotIndexFT.IsIndexed = false;


            if (mappingSettings.StoreSettings.StoreSource)
            {
                doc.Add(new Field(FieldSource, json, storedAndNotIndexFT));
            }

            var longStoredAndNotIndexFT = new FieldType(Int64Field.TYPE_STORED);
            longStoredAndNotIndexFT.IsIndexed = false;

            doc.Add(new Int64Field(FieldTimestamp, DateTime.UtcNow.Ticks, longStoredAndNotIndexFT));

            mappingSettings.ObjectMapper.AddToDocument<TSource>(source, doc);

            return doc;
        }

        /// <summary>
        /// Maps the data from the given Document to an object of type TObject.
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of object to map to.
        /// </typeparam>
        /// <param name="doc">
        /// The Document to map to an object.
        /// </param>
        /// <returns>
        /// An instance of TObject.
        /// </returns>
        public static TObject ToObject<TObject>(this Document doc)
        {
            string actualTypeName = doc.Get(FieldActualType);
            string staticTypeName = doc.Get(FieldStaticType);
            string source = doc.Get(FieldSource);
            string rawTimestamp = doc.Get(FieldTimestamp);

            if (null == source)
            {
                throw new InvalidOperationException(String.Format(
                    "Cannot convert the Document to an object of type <{0}>: The '$source' field is missing.",
                    typeof(TObject)));
            }

            // TODO: Additional checks on object types etc.
            TObject obj = JsonConvert.DeserializeObject<TObject>(source, settings);

            return obj;
        }

        /// <summary>
        /// Maps the data from the given Document to an object of type TObject.
        /// </summary>
        /// <param name="doc">
        /// The Document to map to an object.
        /// </param>
        /// <returns>
        /// An instance of object.
        /// </returns>
        public static object ToObject(this Document doc)
        {
            string actualTypeName = doc.Get(FieldActualType);
            string staticTypeName = doc.Get(FieldStaticType);
            string source = doc.Get(FieldSource);
            string rawTimestamp = doc.Get(FieldTimestamp);

            Type actualType = Type.GetType(actualTypeName);

            return JsonConvert.DeserializeObject(source, actualType, settings);
        }

        #endregion
    }
}
