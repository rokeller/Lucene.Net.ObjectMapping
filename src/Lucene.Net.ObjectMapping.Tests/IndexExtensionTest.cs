using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.ObjectMapping.Tests.Model;
using Lucene.Net.Search;
using Lucene.Net.Store;
using NUnit.Framework;
using System;

namespace Lucene.Net.ObjectMapping.Tests
{
    [TestFixture]
    public class IndexExtensionTest
    {
        private Directory dir;
        private IndexWriter writer;

        [Test]
        public void DeleteDocumentsFromIndex()
        {
            const int NumObjects = 10;
            const int MaxDeletedExclusive = 5;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs());

            writer.DeleteDocuments(
                typeof(TestObject),
                NumericRangeQuery.NewLongRange("Number", null, MaxDeletedExclusive, false, false));
            writer.Commit();

            VerifyTestObjects(NumObjects, MaxDeletedExclusive);
        }

        [Test]
        public void DeleteDocumentsFromIndexWithTypeKindCheck()
        {
            const int NumObjects = 10;
            const int MaxDeletedExclusive = 5;

            WriteTestObjects(NumObjects, obj => obj.ToDocument<object>()); // Static Type: Object, Actual Type: TestObject
            Assert.AreEqual(NumObjects, writer.NumDocs());

            writer.DeleteDocuments(
                typeof(TestObject),
                DocumentObjectTypeKind.Static,
                NumericRangeQuery.NewLongRange("Number", null, MaxDeletedExclusive, false, false));
            writer.Commit();

            // No object must have been deleted yet, because the static type is set to Object, not TestObject.
            VerifyTestObjects(NumObjects, 0);

            writer.DeleteDocuments(
                typeof(TestObject),
                DocumentObjectTypeKind.Actual,
                NumericRangeQuery.NewLongRange("Number", null, MaxDeletedExclusive, false, false));
            writer.Commit();

            VerifyTestObjects(NumObjects, MaxDeletedExclusive);
        }

        [Test]
        public void DeleteDocumentsFromIndexGeneric()
        {
            const int NumObjects = 10;
            const int MaxDeletedExclusive = 5;

            WriteTestObjects(NumObjects, obj => obj.ToDocument());
            Assert.AreEqual(NumObjects, writer.NumDocs());

            writer.DeleteDocuments<TestObject>(
                NumericRangeQuery.NewLongRange("Number", null, MaxDeletedExclusive, false, false));
            writer.Commit();

            VerifyTestObjects(NumObjects, MaxDeletedExclusive);
        }

        [Test]
        public void DeleteDocumentsFromIndexWithTypeKindCheckGeneric()
        {
            const int NumObjects = 10;
            const int MaxDeletedExclusive = 5;

            WriteTestObjects(NumObjects, obj => obj.ToDocument<object>()); // Static Type: Object, Actual Type: TestObject
            Assert.AreEqual(NumObjects, writer.NumDocs());

            writer.DeleteDocuments<TestObject>(
                DocumentObjectTypeKind.Static,
                NumericRangeQuery.NewLongRange("Number", null, MaxDeletedExclusive, false, false));
            writer.Commit();

            // No object must have been deleted yet, because the static type is set to Object, not TestObject.
            VerifyTestObjects(NumObjects, 0);

            writer.DeleteDocuments<TestObject>(
                DocumentObjectTypeKind.Actual,
                NumericRangeQuery.NewLongRange("Number", null, MaxDeletedExclusive, false, false));
            writer.Commit();

            VerifyTestObjects(NumObjects, MaxDeletedExclusive);
        }

        [SetUp]
        public void SetUp()
        {
            dir = new RAMDirectory();
            writer = new IndexWriter(dir, new StandardAnalyzer(Util.Version.LUCENE_30), true, IndexWriter.MaxFieldLength.LIMITED);
        }

        [TearDown]
        public void TearDown()
        {
            writer.Dispose();
            dir.Dispose();
        }

        private void WriteTestObjects(int count, Func<TestObject, Document> converter)
        {
            for (int i = 0; i < count; i++)
            {
                TestObject obj = new TestObject()
                {
                    Number = i,
                    String = String.Format("Test Object {0}", i),
                };

                writer.AddDocument(converter(obj));
            }

            writer.Commit();
        }

        private void VerifyTestObjects(int count, int maxDeletedExclusive)
        {
            Assert.AreEqual(count - maxDeletedExclusive, writer.NumDocs());

            using (IndexReader reader = IndexReader.Open(dir, true))
            {
                for (int i = 0; i < count; i++)
                {
                    if (i < maxDeletedExclusive)
                    {
                        Assert.True(reader.IsDeleted(i));
                    }
                    else
                    {
                        Assert.False(reader.IsDeleted(i));
                        Document doc = reader.Document(i);
                        TestObject obj = doc.ToObject<TestObject>();
                        Assert.AreEqual(i, obj.Number);
                        Assert.AreEqual(String.Format("Test Object {0}", i), obj.String);
                    }
                }
            }
        }
    }
}
