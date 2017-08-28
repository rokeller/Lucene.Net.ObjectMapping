using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.ObjectMapping.Tests.Model;
using Lucene.Net.Search;
using Lucene.Net.Store;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lucene.Net.ObjectMapping.Tests
{
    [TestFixture]
    public class DictionaryTest
    {
        private Directory dir;
        private IndexWriter writer;

        [Test]
        public void Mapping()
        {
            ObjectWithDict obj = Make(1234, new DateTime(2015, 01, 01));
            Document doc = obj.ToDocument();
            Assert.NotNull(doc);
            int remainingFields = 19;

            foreach (IIndexableField field in doc.Fields)
            {
                if (field.FieldType.IsStored)
                {
                    // Ignore stored fields here.
                    continue;
                }

                remainingFields--;

                switch (field.Name)
                {
                    case "Id":
                        Assert.True(field.FieldType.IsTokenized);
                        Assert.True(field.FieldType.IsIndexed);
                        Assert.AreEqual(obj.Id.ToString(), field.GetStringValue());
                        break;

                    case "StringMap.ItemA.Date":
                        Assert.True(field.FieldType.IsTokenized);
                        Assert.True(field.FieldType.IsIndexed);
                        Assert.AreEqual(obj.StringMap["ItemA"].Date.Ticks.ToString(), field.GetStringValue());
                        break;

                    case "StringMap.ItemA.Index":
                        Assert.True(field.FieldType.IsTokenized);
                        Assert.True(field.FieldType.IsIndexed);
                        Assert.AreEqual(obj.StringMap["ItemA"].Index.ToString(), field.GetStringValue());
                        break;

                    case "StringMap.ItemA.Text":
                        Assert.True(field.FieldType.IsTokenized);
                        Assert.True(field.FieldType.IsIndexed);
                        Assert.AreEqual(obj.StringMap["ItemA"].Text, field.GetStringValue());
                        break;

                    case "StringMap.ItemB.Date":
                        Assert.True(field.FieldType.IsTokenized);
                        Assert.True(field.FieldType.IsIndexed);
                        Assert.AreEqual(obj.StringMap["ItemB"].Date.Ticks.ToString(), field.GetStringValue());
                        break;

                    case "StringMap.ItemB.Index":
                        Assert.True(field.FieldType.IsTokenized);
                        Assert.True(field.FieldType.IsIndexed);
                        Assert.AreEqual(obj.StringMap["ItemB"].Index.ToString(), field.GetStringValue());
                        break;

                    case "StringMap.ItemB.Text":
                        Assert.True(field.FieldType.IsTokenized);
                        Assert.True(field.FieldType.IsIndexed);
                        Assert.AreEqual(obj.StringMap["ItemB"].Text, field.GetStringValue());
                        break;

                    case "StringMap.ItemC.Date":
                        Assert.True(field.FieldType.IsTokenized);
                        Assert.True(field.FieldType.IsIndexed);
                        Assert.AreEqual(obj.StringMap["ItemC"].Date.Ticks.ToString(), field.GetStringValue());
                        break;

                    case "StringMap.ItemC.Index":
                        Assert.True(field.FieldType.IsTokenized);
                        Assert.True(field.FieldType.IsIndexed);
                        Assert.AreEqual(obj.StringMap["ItemC"].Index.ToString(), field.GetStringValue());
                        break;

                    case "StringMap.ItemC.Text":
                        Assert.True(field.FieldType.IsTokenized);
                        Assert.True(field.FieldType.IsIndexed);
                        Assert.AreEqual(obj.StringMap["ItemC"].Text, field.GetStringValue());
                        break;

                    case "StringMap.ItemD.Date":
                        Assert.True(field.FieldType.IsTokenized);
                        Assert.True(field.FieldType.IsIndexed);
                        Assert.AreEqual(obj.StringMap["ItemD"].Date.Ticks.ToString(), field.GetStringValue());
                        break;

                    case "StringMap.ItemD.Index":
                        Assert.True(field.FieldType.IsTokenized);
                        Assert.True(field.FieldType.IsIndexed);
                        Assert.AreEqual(obj.StringMap["ItemD"].Index.ToString(), field.GetStringValue());
                        break;

                    case "StringMap.ItemD.Text":
                        Assert.True(field.FieldType.IsTokenized);
                        Assert.True(field.FieldType.IsIndexed);
                        Assert.AreEqual(obj.StringMap["ItemD"].Text, field.GetStringValue());
                        break;

                    case "StringMap.ItemE.Date":
                        Assert.True(field.FieldType.IsTokenized);
                        Assert.True(field.FieldType.IsIndexed);
                        Assert.AreEqual(obj.StringMap["ItemE"].Date.Ticks.ToString(), field.GetStringValue());
                        break;

                    case "StringMap.ItemE.Index":
                        Assert.True(field.FieldType.IsTokenized);
                        Assert.True(field.FieldType.IsIndexed);
                        Assert.AreEqual(obj.StringMap["ItemE"].Index.ToString(), field.GetStringValue());
                        break;

                    case "StringMap.ItemE.Text":
                        Assert.True(field.FieldType.IsTokenized);
                        Assert.True(field.FieldType.IsIndexed);
                        Assert.AreEqual(obj.StringMap["ItemE"].Text, field.GetStringValue());
                        break;

                    case "StringMap.ItemF.Date":
                        Assert.True(field.FieldType.IsTokenized);
                        Assert.True(field.FieldType.IsIndexed);
                        Assert.AreEqual(obj.StringMap["ItemF"].Date.Ticks.ToString(), field.GetStringValue());
                        break;

                    case "StringMap.ItemF.Index":
                        Assert.True(field.FieldType.IsTokenized);
                        Assert.True(field.FieldType.IsIndexed);
                        Assert.AreEqual(obj.StringMap["ItemF"].Index.ToString(), field.GetStringValue());
                        break;

                    case "StringMap.ItemF.Text":
                        Assert.True(field.FieldType.IsTokenized);
                        Assert.True(field.FieldType.IsIndexed);
                        Assert.AreEqual(obj.StringMap["ItemF"].Text, field.GetStringValue());
                        break;

                    default:
                        Assert.Fail("Must get one of the expected fields.");
                        break;
                }
            }

            Assert.AreEqual(0, remainingFields);
        }

        [Test]
        public void Roundtrip()
        {
            ObjectWithDict obj = Make(9876, new DateTime(2015, 01, 01));
            Document doc = obj.ToDocument();

            ObjectWithDict obj2 = doc.ToObject<ObjectWithDict>();

            Assert.AreEqual(obj.Id, obj2.Id);
            Assert.AreEqual(obj.StringMap.Count, obj2.StringMap.Count);

            foreach (KeyValuePair<string, DictValue> item in obj.StringMap)
            {
                if (null == item.Value)
                {
                    Assert.IsNull(obj2.StringMap[item.Key]);
                }
                else
                {
                    Assert.AreEqual(item.Value.Text, obj2.StringMap[item.Key].Text);
                    Assert.AreEqual(item.Value.Date, obj2.StringMap[item.Key].Date);
                }
            }
        }

        [Test]
        public void QueryOnDictionary()
        {
            const int NumObjects = 10;

            Write(NumObjects);
            Assert.AreEqual(NumObjects, writer.MaxDoc);

            using (DirectoryReader reader = DirectoryReader.Open(dir))
            {
                IndexSearcher searcher = new IndexSearcher(reader);

                IQueryable<ObjectWithDict> query = from o in searcher.AsQueryable<ObjectWithDict>()
                                                   where o.StringMap["ItemD"].Text == "d"
                                                   orderby o.Id descending
                                                   select o;

                Assert.AreEqual(NumObjects, query.Count());
                ObjectWithDict[] results = query.ToArray();

                for (int i = 0; i < NumObjects; i++)
                {
                    Assert.AreEqual(NumObjects - 1 - i, results[i].Id);
                }

                query = from o in searcher.AsQueryable<ObjectWithDict>()
                        where o.StringMap["ItemD"].Index == 4 && o.StringMap["ItemF"].Text != "a"
                        orderby o.Id
                        select o;

                Assert.AreEqual(NumObjects, query.Count());
                results = query.ToArray();

                for (int i = 0; i < NumObjects; i++)
                {
                    Assert.AreEqual(i, results[i].Id);
                }
            }
        }

        [SetUp]
        public void SetUp()
        {
            dir = new RAMDirectory();
            writer = new IndexWriter(dir, new IndexWriterConfig(Util.LuceneVersion.LUCENE_48, new StandardAnalyzer(Util.LuceneVersion.LUCENE_48)));
        }

        [TearDown]
        public void TearDown()
        {
            writer.Dispose();
            dir.Dispose();
        }

        private void Write(int count)
        {
            for (int i = 0; i < count; i++)
            {
                DateTime baseDate = DateTime.Now.Date.AddDays(i * 4);
                ObjectWithDict obj = Make(i, baseDate);

                writer.Add(obj);
            }

            writer.Commit();
        }

        private ObjectWithDict Make(int id, DateTime baseDate)
        {
            ObjectWithDict obj = new ObjectWithDict()
            {
                Id = id,
                StringMap = new Dictionary<string, DictValue>()
                {
                    { "ItemNull", null },
                    { "ItemA", new DictValue() { Text = "This is Item A", Index = 1, Date = baseDate.AddDays(1) } },
                    { "ItemB", new DictValue() { Text = "This is Item B", Index = 2, Date = baseDate.AddDays(1) } },
                    { "ItemC", new DictValue() { Text = "This is Item C", Index = 3, Date = baseDate.AddDays(2) } },
                    { "ItemD", new DictValue() { Text = "This is Item D", Index = 4, Date = baseDate.AddDays(3) } },
                    { "ItemE", new DictValue() { Text = "This is Item E", Index = 5, Date = baseDate.AddDays(5) } },
                    { "ItemF", new DictValue() { Text = "This is Item F", Index = 6, Date = baseDate.AddDays(8) } },
                },
            };

            return obj;
        }
    }
}
